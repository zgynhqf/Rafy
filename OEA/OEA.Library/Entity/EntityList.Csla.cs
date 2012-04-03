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

using OEA;
using OEA.ManagedProperty;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace OEA.Library
{
    public abstract partial class EntityList : ManagedPropertyObjectList<Entity>, IDirtyAware, IEntityOrList
    {
        #region Delete and Undelete child

        private ManagedPropertyObjectList<Entity> _deletedList;

        public ManagedPropertyObjectList<Entity> DeletedList
        {
            get
            {
                if (this._deletedList == null) { this._deletedList = new ManagedPropertyObjectList<Entity>(); }

                return this._deletedList;
            }
        }

        private void DeleteChild(Entity child)
        {
            //如果是新的对象，则不需要加入到 DeletedList 列表中。
            if (!child.IsNew)
            {
                // mark the object as deleted
                child.MarkDeleted();

                // and add it to the deleted collection for storage
                this.DeletedList.Add(child);
            }
        }

        #endregion

        #region Insert, Remove, Clear

        /// <summary>
        /// Marks the child object for deletion and moves it to
        /// the collection of deleted objects.
        /// </summary>
        /// <param name="index">Index of the item to remove.</param>
        protected void RemoveItemCsla(int index)
        {
            // when an object is 'removed' it is really
            // being deleted, so do the deletion work
            Entity child = this[index];

            bool oldRaiseListChangedEvents = this.RaiseListChangedEvents;
            try
            {
                this.RaiseListChangedEvents = false;
                base.RemoveItem(index);
            }
            finally
            {
                this.RaiseListChangedEvents = oldRaiseListChangedEvents;
            }

            // the child shouldn't be completely removed,
            // so copy it to the deleted list
            this.DeleteChild(child);

            if (this.RaiseListChangedEvents)
            {
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
            }
        }

        protected override void ClearItems()
        {
            foreach (var item in this) { this.DeleteChild(item); }

            base.ClearItems();
        }

        /// <summary>
        /// Replaces the item at the specified index with
        /// the specified item, first moving the original
        /// item to the deleted list.
        /// </summary>
        /// <param name="index">The zero-based index of the item to replace.</param>
        /// <param name="item">
        /// The new value for the item at the specified index. 
        /// The value can be null for reference types.
        /// </param>
        /// <remarks></remarks>
        protected override void SetItem(int index, Entity item)
        {
            Entity child = null;
            if (this[index] != item) child = this[index];

            // replace the original object with this new
            // object
            bool oldRaiseListChangedEvents = this.RaiseListChangedEvents;
            try
            {
                this.RaiseListChangedEvents = false;

                // set parent reference
                if (!this.SupressSetItemParent) { item.CastTo<IEntityOrList>().SetParent(this); }

                //handle mapping
                //for mapping purposes, you are removing the thing at the index
                //var itemWeAreReplacing = this[index];
                //RemoveFromMap(itemWeAreReplacing);
                //RemoveFromMap(item);

                // add to list
                base.SetItem(index, item);
            }
            finally
            {
                this.RaiseListChangedEvents = oldRaiseListChangedEvents;
            }

            if (child != null) this.DeleteChild(child);

            if (this.RaiseListChangedEvents)
            {
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
        }

        #endregion

        public void AddRange(IEnumerable<Entity> list)
        {
            foreach (var item in list) { this.Add(item); }
        }

        #region IDirtyAware Members

        public bool IsDirty
        {
            get
            {
                // any non-new deletions make us dirty
                foreach (Entity item in DeletedList)
                    if (!item.IsNew)
                        return true;

                // run through all the child objects
                // and if any are dirty then then
                // collection is dirty
                foreach (Entity child in this)
                    if (child.IsDirty)
                        return true;

                return false;
            }
        }

        public void MarkOld()
        {
            ////由于外界已经间接实现了这个功能。
            ////所以暂时不需要添加这块代码了。不过不用删除。可能以后会用到。

            //又用到了。见：DeleteBillCommand

            this.DeletedList.Clear();

            foreach (var child in this)
            {
                if (child.IsDirty)
                {
                    var childEntity = child as IDirtyAware;
                    if (childEntity != null) { childEntity.MarkOld(); }
                }
            }
        }

        #endregion

        #region IEntityOrList Members

        [NonSerialized]
        private IEntityOrList _parent;

        public void SetParent(IEntityOrList parent)
        {
            _parent = parent;
        }

        IEntityOrList IEntityOrList.Parent
        {
            get { return _parent; }
        }

        #endregion

        /// <summary>
        /// Override this method to allow update of a business
        /// object.
        /// </summary>
        internal void DataPortal_Update()
        {
            this.OnSave();
        }
    }

    /// <summary>
    /// copy from csla
    /// </summary>
    public partial class EntityList : ICloneable
    {
        #region ICloneable

        object ICloneable.Clone()
        {
            return GetClone();
        }

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <returns>A new object containing the exact data of the original object.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual object GetClone()
        {
            return OEA.Core.ObjectCloner.Clone(this);
        }

        /// <summary>
        /// Creates a clone of the object.
        /// </summary>
        /// <returns>A new object containing the exact data of the original object.</returns>
        public EntityList Clone()
        {
            return (EntityList)GetClone();
        }

        #endregion

        #region Serialization Notification

        [OnDeserialized]
        private void OnDeserializedHandler(StreamingContext context)
        {
            OnDeserialized();
        }

        /// <summary>
        /// This method is called on a newly deserialized object
        /// after deserialization is complete.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnDeserialized()
        {
            foreach (IEntityOrList child in this)
            {
                child.SetParent(this);
            }

            foreach (IEntityOrList child in DeletedList)
            {
                child.SetParent(this);
            }

            this.NotifyLoaded(null);
        }

        #endregion
    }
}