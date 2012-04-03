using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    public static class MetaExtensions
    {
        #region EntityMetaExtensions - FindParentReferenceProperty

        public static EntityPropertyMeta FindParentReferenceProperty(this EntityMeta meta)
        {
            var result = meta.EntityProperties
                .FirstOrDefault(p => p.ReferenceInfo != null && p.ReferenceInfo.Type == ReferenceType.Parent);
            return result;
        }

        #endregion

        #region EntityMetaExtensions - FindProperty

        /// <summary>
        /// 根据名字查询属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyMeta FindProperty(this EntityMeta meta, IManagedProperty property)
        {
            var ep = meta.Property(property);
            if (ep != null) return ep;

            return meta.ChildrenProperty(property);
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityPropertyMeta Property(this EntityMeta meta, IManagedProperty property)
        {
            return meta.Property(property.GetMetaPropertyName(meta.EntityType));
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityPropertyMeta Property(this EntityMeta meta, string name)
        {
            return meta.EntityProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(name));
        }

        /// <summary>
        /// 根据名字查询关联属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ChildrenPropertyMeta ChildrenProperty(this EntityMeta meta, IManagedProperty property)
        {
            var name = property.GetMetaPropertyName(meta.EntityType);
            return meta.ChildrenProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(name));
        }

        public static ChildrenPropertyMeta ChildrenProperty(this EntityMeta meta, string property)
        {
            return meta.ChildrenProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(property));
        }

        #endregion
    }
}