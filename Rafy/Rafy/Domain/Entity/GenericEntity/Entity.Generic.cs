/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140506
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140506 20:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain
{
    /// <summary>
    /// 一个指定的实体的主键类型的实体基类。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public abstract class Entity<TKey> : Entity, IEntity, IEntityWithId
    {
        #region 构造函数

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Entity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class.
        /// </summary>
        protected Entity() { }

        static Entity()
        {
            KeyProviderField = KeyProviders.Get(typeof(TKey));

            IdProperty.OverrideMeta(typeof(Entity<TKey>), new PropertyMetadata<object>(), m =>
            {
                m.DefaultValue = KeyProviderField.DefaultValue;
            });
        }

        #endregion

        internal static IKeyProvider KeyProviderField;

        [NonSerialized]
        private FastField<TKey> _idFast;

        /// <summary>
        /// 实体标识属性的算法程序。
        /// </summary>
        protected override IKeyProvider IdProvider
        {
            get { return KeyProviderField; }
        }

        /// <summary>
        /// 实体的标识属性。
        /// </summary>
        public new TKey Id
        {
            get { return this.GetProperty(IdProperty, ref this._idFast); }
            set { this.SetProperty(IdProperty, ref this._idFast, value); }
        }

        public override void LoadProperty(IManagedProperty property, object value)
        {
            base.LoadProperty(property, value);

            if (property == IdProperty)
            {
                this.ResetFastField(_idFast);
            }
        }

        /// <summary>
        /// Id 变更后事件。
        /// </summary>
        /// <param name="e">The <see cref="ManagedPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnIdChanged(ManagedPropertyChangedEventArgs e)
        {
            this.ResetFastField(_idFast);

            base.OnIdChanged(e);
        }

        #region 快速字段

        /// <summary>
        /// 使用快速字段完成数据的读取。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="fastfield"></param>
        /// <returns></returns>
        private T GetProperty<T>(IProperty property, ref FastField<T> fastfield)
        {
            if (fastfield == null) fastfield = new FastField<T>();

            if (fastfield.IsEmpty)
            {
                fastfield.Value = (T)this.GetProperty(property);
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
        private void SetProperty<T>(IProperty property, ref FastField<T> fastfield, T value)
        {
            if (fastfield != null)
            {
                fastfield.Value = value;
            }

            this.SetProperty(property, value);
        }

        private void ResetFastField<T>(FastField<T> fastfield)
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
        internal class FastField<T>
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
