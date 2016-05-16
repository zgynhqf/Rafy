/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120412
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120412
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 列表属性
    /// </summary>
    /// <typeparam name="TEntityList"></typeparam>
    public sealed class ListProperty<TEntityList> : Property<TEntityList>, IListProperty, IProperty
        where TEntityList : EntityList
    {
        private Type _listEntityType;

        internal HasManyType _hasManyType;

        internal ListProperty(Type ownerType, string propertyName, ListPropertyMetadata<TEntityList> defaultMeta)
            : base(ownerType, propertyName, defaultMeta) { }

        internal ListProperty(Type ownerType, Type declareType, string propertyName, ListPropertyMetadata<TEntityList> defaultMeta)
            : base(ownerType, declareType, propertyName, defaultMeta) { }

        public new IRafyListPropertyMetadata GetMeta(object owner)
        {
            return base.GetMeta(owner) as IRafyListPropertyMetadata;
        }

        public new IRafyListPropertyMetadata GetMeta(Type ownerType)
        {
            return base.GetMeta(ownerType) as IRafyListPropertyMetadata;
        }

        public override PropertyCategory Category
        {
            get { return PropertyCategory.List; }
        }

        /// <summary>
        /// 列表对应的实体类型
        /// </summary>
        public Type ListEntityType
        {
            get
            {
                if (this._listEntityType == null)
                {
                    this._listEntityType = EntityMatrix.FindByList(this.PropertyType).EntityType;
                }

                return this._listEntityType;
            }
        }

        /// <summary>
        /// 一对多子属性的类型
        /// </summary>
        public HasManyType HasManyType
        {
            get { return _hasManyType; }
        }
    }
}