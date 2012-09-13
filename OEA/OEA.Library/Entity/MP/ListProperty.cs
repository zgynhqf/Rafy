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
using OEA.MetaModel;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// 列表属性
    /// </summary>
    /// <typeparam name="TEntityList"></typeparam>
    public class ListProperty<TEntityList> : Property<TEntityList>, IListProperty
        where TEntityList : EntityList
    {
        private Type _listEntityType;

        public ListProperty(Type ownerType, string propertyName, ListPropertyMetadata<TEntityList> defaultMeta)
            : base(ownerType, propertyName, defaultMeta) { }

        public new IOEAListPropertyMetadata GetMeta(object owner)
        {
            return base.GetMeta(owner) as IOEAListPropertyMetadata;
        }

        public new IOEAListPropertyMetadata GetMeta(Type ownerType)
        {
            return base.GetMeta(ownerType) as IOEAListPropertyMetadata;
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
                    this._listEntityType = EntityConvention.EntityType(this.PropertyType);
                }

                return this._listEntityType;
            }
        }
    }
}