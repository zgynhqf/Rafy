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
using SimpleCsla.Core;
using System.ComponentModel;
using System.Collections.Specialized;
using SimpleCsla;
using System.Runtime.Serialization;
using SimpleCsla.Validation;
using OEA.Serialization.Mobile;
using OEA.Serialization;

namespace OEA.Library
{
    /// <summary>
    /// 原来 Csla 中的 BusinessBase 中的代码，都移动到这个类中。
    /// 
    /// </summary>
    [Serializable]
    public abstract partial class CslaEntity : ManagedPropertyObject, IDirtyAware, IEntityOrList
    {
        #region PersistenceStatus

        [NonSerialized]
        private PersistenceStatus _previousStatusBeforeDeleted = PersistenceStatus.Unchanged;
        private PersistenceStatus _status = PersistenceStatus.Unchanged;

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
        }

        public override void LoadProperty(IManagedProperty property, object value)
        {
            this.SetParentIfChild(value);

            base.LoadProperty(property, value);

            this.MarkOldIfDirty(value);
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

        protected void UpdateChildren(params object[] parameters)
        {
            foreach (var field in this.GetLoadedChildren())
            {
                DataPortal.UpdateChild(field.Value, parameters);
            }
        }

        protected abstract IEnumerable<IManagedPropertyField> GetLoadedChildren();

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

        protected override void OnDeserialized(DesirializedArgs context)
        {
            base.OnDeserialized(context);

            this.ValidationRules.SetTarget(this);

            this.InitializeBusinessRules();

            this.SetChildrenParent();
        }

        private void SetChildrenParent()
        {
            foreach (var field in this.GetLoadedChildren())
            {
                var v = field.Value as IEntityOrList;
                v.SetParent(this);
            }
        }

        #endregion

        public virtual bool IsValid
        {
            get { return IsSelfValid; }
        }

        void SimpleCsla.Server.IDataPortalTarget.MarkNew()
        {
            this.MarkNew();
        }
    }

    /// <summary>
    /// 从 CSLA 中复制过来，没有进行大的修改的代码。
    /// </summary>
    public partial class CslaEntity : ICloneable, SimpleCsla.Server.IDataPortalTarget
    {
        protected CslaEntity()
        {
            this.InitializeBusinessRules();
        }

        #region ICloneable

        object ICloneable.Clone()
        {
            return GetClone();
        }

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <returns>
        /// A new object containing the exact data of the original object.
        /// </returns>
        protected virtual object GetClone()
        {
            return ObjectCloner.Clone(this);
        }

        #endregion

        #region ValidationRules, IsValid

        private ValidationRules _validationRules;

        [NonSerialized]
        private EventHandler _validationCompleteHandlers;

        /// <summary>
        /// Event raised when validation is complete.
        /// </summary>
        public event EventHandler ValidationComplete
        {
            add
            {
                _validationCompleteHandlers = (EventHandler)
                  System.Delegate.Combine(_validationCompleteHandlers, value);
            }
            remove
            {
                _validationCompleteHandlers = (EventHandler)
                  System.Delegate.Remove(_validationCompleteHandlers, value);
            }
        }

        /// <summary>
        /// Raises the ValidationComplete event
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void OnValidationComplete()
        {
            if (_validationCompleteHandlers != null)
                _validationCompleteHandlers(this, EventArgs.Empty);
        }

        private void InitializeBusinessRules()
        {
            AddInstanceBusinessRules();
            if (!(SharedValidationRules.RulesExistFor(this.GetType())))
            {
                lock (this.GetType())
                {
                    if (!(SharedValidationRules.RulesExistFor(this.GetType())))
                    {
                        SharedValidationRules.GetManager(this.GetType(), true);
                        AddBusinessRules();
                    }
                }
            }
        }

        /// <summary>
        /// Provides access to the broken rules functionality.
        /// </summary>
        /// <remarks>
        /// This property is used within your business logic so you can
        /// easily call the AddRule() method to associate validation
        /// rules with your object's properties.
        /// </remarks>
        public ValidationRules ValidationRules
        {
            get
            {
                if (_validationRules == null)
                {
                    _validationRules = new ValidationRules(this);
                }
                else if (_validationRules.Target == null)
                    _validationRules.SetTarget(this);
                return _validationRules;
            }
        }

        /// <summary>
        /// Override this method in your business class to
        /// be notified when you need to set up business
        /// rules.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by CSLA .NET
        /// when your object should associate per-instance
        /// validation rules with its properties.
        /// </remarks>
        protected virtual void AddInstanceBusinessRules()
        {

        }

        /// <summary>
        /// Override this method in your business class to
        /// be notified when you need to set up shared 
        /// business rules.
        /// </summary>
        /// <remarks>
        /// This method is automatically called by CSLA .NET
        /// when your object should associate per-type 
        /// validation rules with its properties.
        /// </remarks>
        protected virtual void AddBusinessRules()
        {

        }

        /// <summary>
        /// Returns <see langword="true" /> if the object is currently 
        /// valid, <see langword="false" /> if the
        /// object has broken rules or is otherwise invalid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default this property relies on the underling ValidationRules
        /// object to track whether any business rules are currently broken for this object.
        /// </para><para>
        /// You can override this property to provide more sophisticated
        /// implementations of the behavior. 
        /// </para>
        /// </remarks>
        /// <returns>A value indicating if the object is currently valid.</returns>
        [Browsable(false)]
        public virtual bool IsSelfValid
        {
            get { return ValidationRules.IsValid; }
        }

        /// <summary>
        /// Provides access to the readonly collection of broken business rules
        /// for this object.
        /// </summary>
        /// <returns>A RulesCollection object.</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public virtual BrokenRulesCollection BrokenRulesCollection
        {
            get { return ValidationRules.GetBrokenRules(); }
        }

        #endregion

        #region Data Access

        /// <summary>
        /// Override this method to load a new business object with default
        /// values from the database.
        /// </summary>
        /// <remarks>
        /// Normally you will overload this method to accept a strongly-typed
        /// criteria parameter, rather than overriding the method with a
        /// loosely-typed criteria parameter.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        [RunLocal]
        protected virtual void DataPortal_Create()
        {
            ValidationRules.CheckRules();
        }

        /// <summary>
        /// Override this method to allow retrieval of an existing business
        /// object based on data in the database.
        /// </summary>
        /// <remarks>
        /// Normally you will overload this method to accept a strongly-typed
        /// criteria parameter, rather than overriding the method with a
        /// loosely-typed criteria parameter.
        /// </remarks>
        /// <param name="criteria">An object containing criteria values to identify the object.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void DataPortal_Fetch(object criteria)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Override this method to allow insertion of a business
        /// object.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void DataPortal_Insert()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Override this method to allow update of a business
        /// object.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void DataPortal_Update()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Override this method to allow deferred deletion of a business object.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void DataPortal_DeleteSelf()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Override this method to allow immediate deletion of a business object.
        /// </summary>
        /// <param name="criteria">An object containing criteria values to identify the object.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void DataPortal_Delete(object criteria)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Called by the server-side DataPortal prior to calling the 
        /// requested DataPortal_XYZ method.
        /// </summary>
        /// <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void DataPortal_OnDataPortalInvoke(DataPortalEventArgs e)
        {

        }

        /// <summary>
        /// Called by the server-side DataPortal after calling the 
        /// requested DataPortal_XYZ method.
        /// </summary>
        /// <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void DataPortal_OnDataPortalInvokeComplete(DataPortalEventArgs e)
        {

        }

        /// <summary>
        /// Override this method to load a new business object with default
        /// values from the database.
        /// </summary>
        /// <remarks>
        /// Normally you will overload this method to accept a strongly-typed
        /// criteria parameter, rather than overriding the method with a
        /// loosely-typed criteria parameter.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        protected virtual void Child_Create()
        {
            ValidationRules.CheckRules();
        }

        /// <summary>
        /// Called by the server-side DataPortal prior to calling the 
        /// requested DataPortal_XYZ method.
        /// </summary>
        /// <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void Child_OnDataPortalInvoke(DataPortalEventArgs e)
        {
        }

        /// <summary>
        /// Called by the server-side DataPortal after calling the 
        /// requested DataPortal_XYZ method.
        /// </summary>
        /// <param name="e">The DataPortalContext object passed to the DataPortal.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", MessageId = "Member")]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void Child_OnDataPortalInvokeComplete(DataPortalEventArgs e)
        {
        }

        #endregion

        #region IDataPortalTarget Members

        void SimpleCsla.Server.IDataPortalTarget.CheckRules()
        {
            ValidationRules.CheckRules();
        }

        void SimpleCsla.Server.IDataPortalTarget.MarkAsChild() { }

        void SimpleCsla.Server.IDataPortalTarget.MarkOld()
        {
            this.MarkOld();
        }

        void SimpleCsla.Server.IDataPortalTarget.DataPortal_OnDataPortalInvoke(DataPortalEventArgs e)
        {
            this.DataPortal_OnDataPortalInvoke(e);
        }

        void SimpleCsla.Server.IDataPortalTarget.DataPortal_OnDataPortalInvokeComplete(DataPortalEventArgs e)
        {
            this.DataPortal_OnDataPortalInvokeComplete(e);
        }

        void SimpleCsla.Server.IDataPortalTarget.Child_OnDataPortalInvoke(DataPortalEventArgs e)
        {
            this.Child_OnDataPortalInvoke(e);
        }

        void SimpleCsla.Server.IDataPortalTarget.Child_OnDataPortalInvokeComplete(DataPortalEventArgs e)
        {
            this.Child_OnDataPortalInvokeComplete(e);
        }

        #endregion
    }
}