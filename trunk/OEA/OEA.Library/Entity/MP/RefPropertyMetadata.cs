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
using OEA.MetaModel;
using System.ComponentModel;

namespace OEA.Library
{
    /// <summary>
    /// 引用属性元数据
    /// </summary>
    public interface IRefPropertyMetadata : IOEARefPropertyMetadata
    {
        /// <summary>
        /// 是否需要序列化实体
        /// </summary>
        bool SerializeEntity { get; }

        /// <summary>
        /// 是否通知引用实体变更
        /// </summary>
        bool NotifyRefEntityChanged { get; }

        /// <summary>
        /// Id 变更前事件
        /// </summary>
        RefIdChangingCallBack IdChangingCallBack { get; }

        /// <summary>
        /// Id 变更后事件
        /// </summary>
        RefIdChangedCallBack IdChangedCallBack { get; }

        /// <summary>
        /// 实体变更前事件
        /// </summary>
        RefEntityChangingCallBack EntityChangingCallBack { get; }

        /// <summary>
        /// 实体变更后事件
        /// </summary>
        RefEntityChangedCallBack EntityChangedCallBack { get; }
    }

    /// <summary>
    /// 泛型版本的引用属性元数据
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class RefPropertyMetadata<TEntity> : PropertyMetadata<ILazyEntityRef<TEntity>>, IRefPropertyMetadata
        where TEntity : Entity
    {
        public RefPropertyMetadata(RefPropertyMeta core, bool nullable)
        {
            this.Core = core;
            this.Nullable = nullable;
        }

        internal RefPropertyMeta Core { get; private set; }

        public bool Nullable { get; private set; }

        #region IRefPropertyMetadata Members

        string IOEARefPropertyMetadata.IdProperty
        {
            get { return this.Core.IdProperty; }
        }

        string IOEARefPropertyMetadata.RefEntityProperty
        {
            get { return this.Core.RefEntityProperty; }
        }

        ReferenceType IOEARefPropertyMetadata.ReferenceType
        {
            get { return this.Core.ReferenceType; }
        }

        bool IRefPropertyMetadata.SerializeEntity
        {
            get { return this.Core.SerializeEntity; }
        }

        bool IRefPropertyMetadata.NotifyRefEntityChanged
        {
            get { return this.Core.NotifyRefEntityChanged; }
        }

        RefIdChangingCallBack IRefPropertyMetadata.IdChangingCallBack
        {
            get { return this.Core.IdChangingCallBack; }
        }

        RefEntityChangingCallBack IRefPropertyMetadata.EntityChangingCallBack
        {
            get { return this.Core.RefEntityChangingCallBack; }
        }

        RefIdChangedCallBack IRefPropertyMetadata.IdChangedCallBack
        {
            get { return this.Core.IdChangedCallBack; }
        }

        RefEntityChangedCallBack IRefPropertyMetadata.EntityChangedCallBack
        {
            get { return this.Core.RefEntityChangedCallBack; }
        }

        #endregion
    }

    /// <summary>
    /// 非泛型的外键元数据
    /// </summary>
    public class RefPropertyMeta
    {
        public RefPropertyMeta()
        {
            this.ReferenceType = ReferenceType.Normal;
            this.SerializeEntity = true;
            this.NotifyRefEntityChanged = true;
        }

        /// <summary>
        /// 对应的 Id 属性名
        /// </summary>
        public string IdProperty { get; set; }

        /// <summary>
        /// 对应的引用实体属性名
        /// </summary>
        public string RefEntityProperty { get; set; }

        /// <summary>
        /// 引用类型
        /// </summary>
        public ReferenceType ReferenceType { get; set; }

        /// <summary>
        /// Id 变更前事件
        /// </summary>
        public RefIdChangingCallBack IdChangingCallBack { get; set; }

        /// <summary>
        /// Id 变更后事件
        /// </summary>
        public RefIdChangedCallBack IdChangedCallBack { get; set; }

        /// <summary>
        /// 实体变更前事件
        /// </summary>
        public RefEntityChangingCallBack RefEntityChangingCallBack { get; set; }

        /// <summary>
        /// 实体变更后事件
        /// </summary>
        public RefEntityChangedCallBack RefEntityChangedCallBack { get; set; }

        /// <summary>
        /// 是否需要序列化引用实体
        /// </summary>
        public bool SerializeEntity { get; set; }

        /// <summary>
        /// 是否通知引用实体变更
        /// </summary>
        public bool NotifyRefEntityChanged { get; set; }

        /// <summary>
        /// 实例加载器（使用外键拥有者作为加载上下文）
        /// </summary>
        public Func<int, object, Entity> InstaceLoader { get; set; }

        /// <summary>
        /// 静态方法加载器
        /// </summary>
        public Func<int, Entity> StaticLoader { get; set; }
    }

    /// <summary>
    /// 引用 ID 变更前事件参数
    /// </summary>
    public class RefIdChangingEventArgs : CancelEventArgs
    {
        private int _value;

        internal RefIdChangingEventArgs(int value)
        {
            this._value = value;
        }

        public int Value
        {
            get { return this._value; }
        }

        public int? NullableValue
        {
            get
            {
                if (this._value == default(int)) return null;
                return this._value;
            }
        }
    }

    /// <summary>
    /// 引用 ID 变更事件参数
    /// </summary>
    public class RefIdChangedEventArgs : EventArgs
    {
        private int _oldId;

        private int _newId;

        internal RefIdChangedEventArgs(int oldId, int newId)
        {
            this._oldId = oldId;
            this._newId = newId;
        }

        public int OldId
        {
            get { return this._oldId; }
        }

        public int NewId
        {
            get { return this._newId; }
        }

        public int? OldNullableId
        {
            get
            {
                if (this._oldId == default(int)) return null;
                return this._oldId;
            }
        }

        public int? NewNullableId
        {
            get
            {
                if (this._newId == default(int)) return null;
                return this._newId;
            }
        }
    }

    /// <summary>
    /// 引用 Entity 变更前事件参数
    /// </summary>
    public class RefEntityChangingEventArgs : CancelEventArgs
    {
        private Entity _newEntity;

        internal RefEntityChangingEventArgs(Entity newEntity)
        {
            this._newEntity = newEntity;
        }

        public Entity Value
        {
            get { return this._newEntity; }
        }
    }

    /// <summary>
    /// 引用 Entity 变更事件参数
    /// </summary>
    public class RefEntityChangedEventArgs : EventArgs
    {
        private Entity _oldEntity;

        private Entity _newEntity;

        internal RefEntityChangedEventArgs(Entity oldEntity, Entity newEntity)
        {
            this._oldEntity = oldEntity;
            this._newEntity = newEntity;
        }

        public Entity OldEntity
        {
            get { return this._oldEntity; }
        }

        public Entity NewEntity
        {
            get { return this._newEntity; }
        }
    }

    /// <summary>
    /// Id 变更前事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RefIdChangingCallBack(Entity sender, RefIdChangingEventArgs e);

    /// <summary>
    /// Id 变更后事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RefIdChangedCallBack(Entity sender, RefIdChangedEventArgs e);

    /// <summary>
    /// 实体变更前事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RefEntityChangingCallBack(Entity sender, RefEntityChangingEventArgs e);

    /// <summary>
    /// 实体变更后事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void RefEntityChangedCallBack(Entity sender, RefEntityChangedEventArgs e);
}