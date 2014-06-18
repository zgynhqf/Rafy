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

namespace Rafy.Domain
{
    /// <summary>
    /// 原来 Csla 中的 BusinessBase 中的代码，都移动到这个类中。
    /// </summary>
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
        /// 实体当前的状态。
        /// </summary>
        public PersistenceStatus PersistenceStatus
        {
            get { return this._status; }
        }

        /// <summary>
        /// 判断本对象是否已经被标记为删除。
        /// 
        /// 标记为删除的对象，在仓库中保存时，会被永久删除。
        /// </summary>
        public bool IsDeleted
        {
            get { return this._status == PersistenceStatus.Deleted; }
        }

        /// <summary>
        /// 判断本对象是否是刚构造出来的。
        /// </summary>
        public bool IsNew
        {
            get { return this._status == PersistenceStatus.New; }
        }

        /// <summary>
        /// 判断本对象是否被修改了属性。
        /// </summary>
        public bool IsModified
        {
            get { return this._status == PersistenceStatus.Modified; }
        }

        /// <summary>
        /// 返回当前对象（非组合）是否本身需要保存。
        /// </summary>
        public bool IsSelfDirty
        {
            get { return this._status != PersistenceStatus.Unchanged; }
        }

        /// <summary>
        /// 标记当前对象为需要保存的状态。
        /// 只有<see cref="PersistenceStatus"/> 为 Unchanged 状态时，才会调用 MarkModified 进行标记。这是因为其它状态已经算是 Dirty 了。
        /// </summary>
        public void MarkSelfDirty()
        {
            //只有 Unchanged 状态时，才需要标记，这是因为其它状态已经算是 Dirty 了。
            if (this._status == PersistenceStatus.Unchanged) { this.MarkModified(); }
        }

        /// <summary>
        /// 把当前对象标记为需要删除状态。
        /// </summary>
        public void MarkDeleted()
        {
            SetFlags(EntitySerializableFlags.IsDeleted, true);
        }

        /// <summary>
        /// 清空实体的删除状态，使其还原到删除之前的状态。
        /// </summary>
        public void RevertDeleted()
        {
            SetFlags(EntitySerializableFlags.IsDeleted, false);
        }

        /// <summary>
        /// 把当前对象标记为一个全新的刚创建的对象。
        /// </summary>
        public void MarkNew()
        {
            SetFlags(EntitySerializableFlags.IsDeleted, false);
            SetFlags(EntitySerializableFlags.IsNew, true);
        }

        /// <summary>
        /// 把当前对象标记为已经被更改的状态。
        /// 该状态的对象的所有数据，将会被更新到仓库中。
        /// </summary>
        private void MarkModified()
        {
            SetFlags(EntitySerializableFlags.DeletedOrNew, false);
            SetFlags(EntitySerializableFlags.IsModified, true);
        }

        /// <summary>
        /// 把当前对象标记为未更改状态。（即刚从仓库中取出的状态。）
        /// </summary>
        public void MarkUnchanged()
        {
            SetFlags(EntitySerializableFlags.PersistenceAll, false);
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
                if (this.IsSelfDirty) return true;

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
        public void MarkSaved()
        {
            this.MarkUnchanged();

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
                    (value as EntityList).InitListProperty(property as IListProperty);
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
                this.MarkSelfDirty();

                this.NotifyIfInRedundancyPath(e);
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

        //    info.AddValue("_status", this._status);
        //    info.AddValue("_lastStatus", this._lastStatus);
        //    info.AddValue("_isChild", this._isChild);
        //    info.AddValue("_validationRules", this._validationRules);
        //}

        //protected override void OnSetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.OnSetObjectData(info, context);

        //    this._status = info.GetValue<PersistenceStatus>("_status");
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