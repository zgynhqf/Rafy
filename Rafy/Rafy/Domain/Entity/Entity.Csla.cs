/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain
{
    //原来 Csla 中的 BusinessBase 中的代码，都移动到这个类中。
    public abstract partial class Entity : IDirtyAware, IDomainComponent
    {
        #region Status

        /// <summary>
        /// 所有需要序列化的 bool 值，都可以存储在这里。减少传输数据量。
        /// </summary>
        private uint _flags = (uint)EntitySerializableFlags.IsNew;

        private bool GetFlags(EntitySerializableFlags flag)
        {
            var flagValue = (uint)flag;
            return (_flags & flagValue) == flagValue;
        }

        private void SetFlags(EntitySerializableFlags flag, bool value)
        {
            if (value)
            {
                _flags |= (uint)flag;
            }
            else
            {
                _flags &= (uint)(EntitySerializableFlags.AllMask ^ flag);
            }
        }

        private PersistenceStatus _status
        {
            get
            {
                //注意，以下代码的顺序注定了实体状态的优先级顺序。
                if (GetFlags(EntitySerializableFlags.IsDeleted))
                {
                    return PersistenceStatus.Deleted;
                }
                if (GetFlags(EntitySerializableFlags.IsNew))
                {
                    return PersistenceStatus.New;
                }
                return GetFlags(EntitySerializableFlags.IsModified) ?
                    PersistenceStatus.Modified : PersistenceStatus.Unchanged;
            }
        }

        /// <summary>
        /// 获取或设置实体当前的持久化状态。
        /// 保存实体时，会根据这个状态来进行对应的增、删、改的操作。
        /// </summary>
        public PersistenceStatus PersistenceStatus
        {
            get { return _status; }
            set
            {
                switch (value)
                {
                    case PersistenceStatus.Unchanged:
                        //把当前对象标记为未更改状态。（即刚从仓库中取出的状态。）
                        SetFlags(EntitySerializableFlags.PersistenceAll, false);
                        break;
                    case PersistenceStatus.Modified:
                        //把当前对象标记为已经被更改的状态。
                        //该状态的对象的所有数据，将会被更新到仓库中。
                        SetFlags(EntitySerializableFlags.DeletedOrNew, false);
                        SetFlags(EntitySerializableFlags.IsModified, true);
                        break;
                    case PersistenceStatus.New:
                        //把当前对象标记为一个全新的刚创建的对象。
                        SetFlags(EntitySerializableFlags.IsDeleted, false);
                        SetFlags(EntitySerializableFlags.IsNew, true);
                        break;
                    case PersistenceStatus.Deleted:
                        //把当前对象标记为需要删除状态。
                        SetFlags(EntitySerializableFlags.IsDeleted, true);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// 判断本对象是否已经被标记为删除。
        /// 
        /// 标记为删除的对象，在仓库中保存时，会被永久删除。
        /// </summary>
        internal bool IsDeleted
        {
            get { return _status == PersistenceStatus.Deleted; }
        }

        /// <summary>
        /// 判断本对象是否是刚构造出来的。
        /// </summary>
        internal bool IsNew
        {
            get { return _status == PersistenceStatus.New; }
        }

        /// <summary>
        /// 标记当前对象为需要保存的状态。
        /// 
        /// 只有实体的状态是 Unchanged 状态时（其它状态已经算是 Dirty 了），调用本方法才会把实体的状态改为 Modified。
        /// </summary>
        void IEntityWithStatus.MarkModifiedIfUnchanged()
        {
            //只有 Unchanged 状态时，才需要标记，这是因为其它状态已经算是 Dirty 了。
            if (_status == PersistenceStatus.Unchanged)
            {
                this.PersistenceStatus = PersistenceStatus.Modified;
            }
        }

        /// <summary>
        /// 清空实体的 IsDeleted 状态，使其还原到删除之前的状态。
        /// </summary>
        void IEntityWithStatus.RevertDeletedStatus()
        {
            SetFlags(EntitySerializableFlags.IsDeleted, false);
        }

        /// <summary>
        /// 清空实体的 IsNew 状态，使其还原到之前的状态。
        /// </summary>
        void IEntityWithStatus.RevertNewStatus()
        {
            SetFlags(EntitySerializableFlags.IsNew, false);
        }

        #endregion

        #region IDirtyAware

        /// <summary>
        /// 如果当前组合对象树中任意部分变更了，则返回 true。
        /// </summary>
        public bool IsDirty
        {
            get
            {
                if (this.PersistenceStatus != PersistenceStatus.Unchanged) return true;

                foreach (var field in this.GetLoadedChildren())
                {
                    var child = field.Value;
                    if (child.IsDirty) return true;
                }

                //树的父子节点，一般算是这个节点的聚合关系。
                if (_treeChildren != null && _treeChildren.IsDirty) { return true; }

                return false;
            }
        }

        /// <summary>
        /// 递归将整个组合对象树都标记为未变更状态。
        /// </summary>
        void IDirtyAware.MarkSaved()
        {
            this.PersistenceStatus = PersistenceStatus.Unchanged;

            var enumerator = this.GetLoadedChildren();
            while (enumerator.MoveNext())
            {
                var child = enumerator.Current.Value;
                child.MarkSaved();
            }

            if (_treeChildren != null)
            {
                _treeChildren.MarkSaved();
            }
        }

        #endregion

        #region LoadProperty

        /// <summary>
        /// LoadProperty 以最快的方式直接加载值，不发生 PropertyChanged 事件。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public override void LoadProperty(IManagedProperty property, object value)
        {
            if (property == IdProperty || property == TreePIdProperty && value != null)
            {
                //由于 Id 属性的托管属性类型是 object，这里需要强制为具体的主键类型。
                value = TypeHelper.CoerceValue(this.IdProvider.KeyType, value);
            }

            base.LoadProperty(property, value);

            this.OnChildLoaded(property, value);
        }

        private void OnChildLoaded(IManagedProperty property, object value)
        {
            var component = value as IDomainComponent;
            if (component != null)
            {
                component.SetParent(this);
                component.MarkSaved();

                if (property is IListProperty)
                {
                    var list = value as EntityList;
                    list.ResetItemParent = true;
                    list.InitListProperty(property as IListProperty);
                }
            }
        }

        #endregion

        /// <summary>
        /// 这个事件不可以屏敝，否则状态会出问题。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(ManagedPropertyChangedEventArgs e)
        {
            var meta = e.Property.DefaultMeta as IPropertyMetadata;
            if (meta.AffectStatus)
            {
                (this as IEntityWithStatus).MarkModifiedIfUnchanged();

                this.NotifyIfInRedundancyPath(e.Property as IProperty);
            }

            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// 同步当前实体的 Id 到组合子实体及子节点中。
        /// </summary>
        internal void SyncIdToChildren()
        {
            foreach (var field in this.GetLoadedChildren())
            {
                var children = field.Value as EntityList;
                if (children != null)
                {
                    children.SyncParentEntityId(this);
                }
                else
                {
                    throw new NotImplementedException("暂时没有实现对单个子实体引用属性的 Id 同步。");
                }
            }

            this.SyncTreeChildrenPId();
        }

        /// <summary>
        /// 获取所有已经加载的组合子的字段集合。
        /// 
        /// 返回的字段的值必须是 IEntityOrList 类型。
        /// 子有可能是集合、也有可能只是一个单一的实体。只会是这两种情况。
        /// </summary>
        /// <returns></returns>
        public LoadedChildrenEnumerator GetLoadedChildren()
        {
            return new LoadedChildrenEnumerator(this);
        }

        #region EnumerateLoadedChildren

        public struct LoadedChildrenEnumerator
        {
            private Entity _entity;
            private IList<IProperty> _childProperties;
            private int _index;
            private ChildPropertyField _current;

            internal LoadedChildrenEnumerator(Entity entity)
            {
                _entity = entity;

                //Entity 有可能是一个 Criteria，这时会没有仓库。
                var repo = entity.FindRepository();
                if (repo != null)
                {
                    _childProperties = repo.GetChildProperties();
                }
                else
                {
                    _childProperties = new IProperty[0];
                }
                _index = -1;
                _current = new ChildPropertyField();
            }

            public ChildPropertyField Current
            {
                get { return _current; }
            }

            public bool MoveNext()
            {
                while (true)
                {
                    _index++;
                    if (_index >= _childProperties.Count) { break; }

                    var property = _childProperties[_index];
                    var value = _entity.GetProperty(property) as IDomainComponent;
                    if (value != null)
                    {
                        _current = new ChildPropertyField(property, value);
                        return true;
                    }
                }

                return false;
            }

            public LoadedChildrenEnumerator GetEnumerator()
            {
                //添加此方法，使得可以使用 foreach 循环
                return this;
            }
        }

        public struct ChildPropertyField
        {
            private IProperty _property;
            private IDomainComponent _value;

            public ChildPropertyField(IProperty property, IDomainComponent value)
            {
                _property = property;
                _value = value;
            }

            public IProperty Property
            {
                get { return _property; }
            }
            public IDomainComponent Value
            {
                get { return _value; }
            }
        }

        #endregion

        #region IEntityOrList Members

        /// <summary>
        /// 实体所在的当前列表对象。
        /// 
        /// 虽然一个实体可以存在于多个集合中，但是，它只保留一个主要集合的引用，见：<see cref="EntityList.ResetItemParent"/>。
        /// </summary>
        EntityList IEntity.ParentList
        {
            get { return _parent as EntityList; }
        }

        [NonSerialized]
        private IDomainComponent _parent;

        /// <summary>
        /// Provide access to the parent reference for use
        /// in child object code.
        /// </summary>
        /// <remarks>
        /// This value will be Nothing for root objects.
        /// </remarks>
        IDomainComponent IDomainComponent.Parent
        {
            get { return _parent; }
        }

        void IDomainComponent.SetParent(IDomainComponent parent)
        {
            _parent = parent;
        }

        internal void DisconnectFromParent()
        {
            _parent = null;
        }

        #endregion

        #region Serialization / Deserialization

        /// <summary>
        /// 反序列化事件。
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void OnDeserialized(DesirializedArgs context)
        {
            base.OnDeserialized(context);

            this.SetChildrenParent_OnDeserializaion();
        }

        private void SetChildrenParent_OnDeserializaion()
        {
            var enumerator = this.GetLoadedChildren();
            while (enumerator.MoveNext())
            {
                var child = enumerator.Current.Value;
                child.SetParent(this);
            }

            if (_treeChildren != null)
            {
                _treeChildren.NotifyDeserialized(this);
            }
        }

        //protected override void OnGetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.OnGetObjectData(info, context);

        //    info.AddValue("_status", _status);
        //    info.AddValue("_lastStatus", this._lastStatus);
        //    info.AddValue("_isChild", this._isChild);
        //    info.AddValue("_validationRules", this._validationRules);
        //}

        //protected override void OnSetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.OnSetObjectData(info, context);

        //    _status = info.GetValue<PersistenceStatus>("_status");
        //    this._lastStatus = info.GetValue<PersistenceStatus?>("_lastStatus");
        //    this._isChild = info.GetBoolean("_isChild");
        //    this._validationRules = info.GetValue<ValidationRules>("_validationRules");

        //    ValidationRules.SetTarget(this);

        //    InitializeBusinessRules();

        //    this.SetChildrenParent();
        //}

        #endregion
    }
}