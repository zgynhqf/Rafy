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
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

using System.ComponentModel;
using System.Collections.Specialized;

using System.Runtime.Serialization;
using OEA.Serialization.Mobile;
using OEA.Serialization;

namespace OEA.Library
{
    /// <summary>
    /// 原来 Csla 中的 BusinessBase 中的代码，都移动到这个类中。
    /// 
    /// </summary>
    public abstract partial class Entity : IDirtyAware, IEntityOrList
    {
        #region PersistenceStatus

        [NonSerialized]
        private PersistenceStatus _previousStatusBeforeDeleted = PersistenceStatus.Unchanged;
        private PersistenceStatus _status = PersistenceStatus.New;

        public PersistenceStatus Status
        {
            get { return this._status; }
            set
            {
                if (value != this._status)
                {
                    if (value == PersistenceStatus.Deleted)
                    {
                        this._previousStatusBeforeDeleted = this._status;
                    }

                    this._status = value;
                }
            }
        }

        public bool IsNew
        {
            get { return this.Status == PersistenceStatus.New; }
        }

        public bool IsDeleted
        {
            get { return this.Status == PersistenceStatus.Deleted; }
        }

        public void MarkNew()
        {
            this.Status = PersistenceStatus.New;
        }

        public void MarkDirty()
        {
            if (this.Status != PersistenceStatus.New)
            {
                this.Status = PersistenceStatus.Modified;
            }
        }

        public virtual void MarkDeleted()
        {
            this.Status = PersistenceStatus.Deleted;
        }

        public void RevertDeleted()
        {
            this._status = this._previousStatusBeforeDeleted;
        }

        protected void MarkModified()
        {
            if (this.Status == PersistenceStatus.Unchanged) { this.Status = PersistenceStatus.Modified; }
        }

        #endregion

        #region IDirtyAware

        public bool IsDirty
        {
            get
            {
                if (this.IsSelfDirty) return true;

                var nonDefaultValues = this.GetNonDefaultPropertyValues();
                foreach (var field in nonDefaultValues)
                {
                    var value = field.Value as IDirtyAware;
                    if (value != null && value.IsDirty) return true;
                }

                return false;
            }
        }

        public virtual bool IsSelfDirty
        {
            get { return this.Status != PersistenceStatus.Unchanged; }
        }

        public virtual void MarkOld()
        {
            this.Status = PersistenceStatus.Unchanged;

            foreach (var field in this.GetNonDefaultPropertyValues())
            {
                var value = field.Value as IDirtyAware;
                if (value != null && value.IsDirty) value.MarkOld();
            }
        }

        #endregion

        #region LoadProperty

        public override void LoadProperty<TPropertyType>(ManagedProperty<TPropertyType> property, TPropertyType value)
        {
            this.SetParentIfChild(value);

            base.LoadProperty<TPropertyType>(property, value);

            this.MarkOldIfDirty(value);

            this.OnPropertyLoaded(property);
        }

        public override void LoadProperty(IManagedProperty property, object value)
        {
            this.SetParentIfChild(value);

            base.LoadProperty(property, value);

            this.MarkOldIfDirty(value);

            this.OnPropertyLoaded(property);
        }

        private void SetParentIfChild(object value)
        {
            var component = value as IEntityOrList;
            if (component != null) { component.SetParent(this); }
        }

        private void MarkOldIfDirty(object value)
        {
            var component = value as IDirtyAware;
            if (component != null) { component.MarkOld(); }
        }

        #endregion

        /// <summary>
        /// 这个事件不可以屏敝，否则状态会出问题。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(IManagedPropertyChangedEventArgs e)
        {
            if (e.Source != ManagedPropertyChangedSource.FromPersistence)
            {
                this.MarkModified();
            }

            base.OnPropertyChanged(e);
        }

        protected void UpdateChildren()
        {
            foreach (var field in this.GetLoadedChildren())
            {
                DataPortal.UpdateChild(field.Value, this);
            }
        }

        /// <summary>
        /// 获取所有已经加载的子的字段集合。
        /// 
        /// 子有可能是集合、也有可能只是一个实体。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IManagedPropertyField> GetLoadedChildren()
        {
            var fields = this.GetNonDefaultPropertyValues();

            foreach (var field in fields)
            {
                var meta = field.Property.GetMeta(this) as IPropertyMetadata;
                if (meta.IsChild) { yield return field; }
            }
        }

        #region IEntityOrList Members

        [NonSerialized]
        private IEntityOrList _parent;

        /// <summary>
        /// Provide access to the parent reference for use
        /// in child object code.
        /// </summary>
        /// <remarks>
        /// This value will be Nothing for root objects.
        /// </remarks>
        IEntityOrList IEntityOrList.Parent
        {
            get { return _parent; }
        }

        void IEntityOrList.SetParent(IEntityOrList parent)
        {
            _parent = parent;
        }

        private void SetChildrenParent_OnDeserializaion()
        {
            foreach (var field in this.GetLoadedChildren())
            {
                var v = field.Value as IEntityOrList;
                v.SetParent(this);
            }
        }

        #endregion

        #region Serialization / Deserialization

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