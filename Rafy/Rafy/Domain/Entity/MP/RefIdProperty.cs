/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121120 20:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121120 20:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain
{
    /// <summary>
    /// 引用实体属性的 Id 标记
    /// </summary>
    public sealed class RefIdProperty<TKey> : Property<TKey>, IRefIdProperty
    {
        internal RefIdProperty(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<TKey> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        internal RefIdProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<TKey> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        public override PropertyCategory Category
        {
            get { return PropertyCategory.ReferenceId; }
        }

        /// <summary>
        /// 实体引用的类型
        /// </summary>
        public ReferenceType ReferenceType { get; internal set; }

        /// <summary>
        /// 返回对应的引用实体属性。
        /// </summary>
        public IRefEntityProperty RefEntityProperty
        {
            get
            {
                var value = (this as IManagedPropertyInternal).RefEntityProperty;
                if (value == null)
                {
                    throw new InvalidProgramException(string.Format("没有为 '{0}.{1}' 属性编写对应的实体引用属性！", this.OwnerType.Name, this.Name));
                }
                return value;
            }
        }

        /// <summary>
        /// 该引用属性是否可空
        /// </summary>
        public bool Nullable => this.RefEntityProperty.Nullable;

        /// <summary>
        /// 引用实体的类型
        /// </summary>
        public Type RefEntityType => this.RefEntityProperty.RefEntityType;

        /// <summary>
        /// 引用的实体的主键的算法程序。
        /// </summary>
        public IKeyProvider KeyProvider => this.RefEntityProperty.KeyProvider;

        IManagedProperty IRefProperty.RefKeyProperty => this;

        IManagedProperty IRefProperty.KeyPropertyOfRefEntity => this.RefEntityProperty.KeyPropertyOfRefEntity;
    }
}
