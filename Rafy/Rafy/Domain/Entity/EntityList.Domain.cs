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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain
{
    partial class EntityList
    {
        /// <summary>
        /// 通过 Id 来查找某个实体。
        /// </summary>
        /// <param name="id">需要查找的实体的 id 值。</param>
        /// <param name="coreceType">
        /// 如果传入的 id 的类型与实体的类型不一致，则强制转换为一致的类型。
        /// 如果不确定传入的类型是否一致，则可以指定这个参数为 true。
        /// </param>
        /// <returns></returns>
        public Entity Find(object id, bool coreceType = false)
        {
            if (this.Count <= 0) return null;

            if (coreceType)
            {
                var idType = (this[0] as IEntityWithId).IdProvider.KeyType;
                id = TypeHelper.CoerceValue(idType, id);
            }

            for (int i = 0, c = this.Count; i < c; i++)
            {
                var e = this[i];
                if (e.Id.Equals(id)) return e;
            }
            return null;
        }

        #region Delete and Undelete child

        private ManagedPropertyObjectList<Entity> _deletedList;

        /// <summary>
        /// 本列表中已经被移除的所有对象。
        /// 这些对象将会从仓库中删除。
        /// </summary>
        public IList<Entity> DeletedList
        {
            get
            {
                if (_deletedList == null)
                {
                    _deletedList = new ManagedPropertyObjectList<Entity>();
                }

                return _deletedList;
            }
        }

        internal ManagedPropertyObjectList<Entity> DeletedListField
        {
            get { return _deletedList; }
            set { _deletedList = null; }
        }

        #endregion

        #region IDirtyAware Members

        /// <summary>
        /// 返回当前列表是否需要保存。
        /// </summary>
        public virtual bool IsDirty
        {
            get
            {
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var child = this[i];
                    if (child.IsDirty) return true;
                }

                if (_deletedList != null && _deletedList.Count > 0) return true;

                return false;
            }
        }

        /// <summary>
        /// 将当前列表中所有的组合对象树都标记为未变更状态。
        /// </summary>
        public void MarkSaved()
        {
            _deletedList = null;

            for (int i = 0, c = this.Count; i < c; i++)
            {
                var entity = this[i];

                var child = entity as IDirtyAware;
                child.MarkSaved();
            }
        }

        #endregion

        #region IDomainComponent Members

        [NonSerialized]
        private IDomainComponent _parent;

        void IDomainComponent.SetParent(IDomainComponent parent)
        {
            this._parent = parent;
        }

        IDomainComponent IDomainComponent.Parent
        {
            get { return this._parent; }
        }

        /// <summary>
        /// 获取此集合所属于的父实体。
        /// </summary>
        public Entity Parent
        {
            get { return this._parent as Entity; }
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
            foreach (IDomainComponent child in this)
            {
                child.SetParent(this);
            }

            if (_deletedList != null)
            {
                foreach (IDomainComponent child in _deletedList)
                {
                    child.SetParent(this);
                }
            }

            this.SetRepo(null);
        }

        #endregion
    }
}