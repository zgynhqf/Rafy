using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.MetaModel.Attributes;
using OEA.ManagedProperty;

namespace OEA.MetaModel.View
{
    public static class MetaExtensions
    {
        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Property(this EntityViewMeta meta, IManagedProperty property)
        {
            return meta.Property(property.GetMetaPropertyName(meta.EntityType));
        }

        /// <summary>
        /// 根据名字查询实体属性（忽略大小写）
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static EntityPropertyViewMeta Property(this EntityViewMeta meta, string name)
        {
            return meta.EntityProperties.FirstOrDefault(item => item.Name.EqualsIgnorecase(name));
        }

        public static string TitlePath(this ReferenceViewInfo reference)
        {
            var entityViewInfo = reference.RefTypeDefaultView;
            if (string.IsNullOrEmpty(reference.RefEntityProperty))
            {
                return entityViewInfo.TitleProperty.Name;
            }
            return reference.RefEntityProperty + "." + entityViewInfo.TitleProperty.Name;
        }
    }
}