using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// 引用实体属性的扩展方法。
    /// </summary>
    public static class OEARefPropertyExtension
    {
        /// <summary>
        /// 获取某个托管属性对应于 ownerType，在 OEA 元数据中被使用的属性名
        /// </summary>
        /// <param name="property"></param>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        public static string GetMetaPropertyName(this IManagedProperty property, Type ownerType)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            string propertyName = property.Name;

            var refMP = property as IOEARefProperty;
            if (refMP != null) { propertyName = refMP.GetIdProperty(ownerType); }

            return propertyName;
        }

        /// <summary>
        /// 获取对应实体的 Id 属性名
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static string GetIdProperty(this IOEARefProperty property, object owner)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (owner == null) throw new ArgumentNullException("owner");

            return property.GetMeta(owner.GetType()).IdProperty;
        }

        /// <summary>
        /// 获取对应实体类型的 Id 属性名
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        public static string GetIdProperty(this IOEARefProperty property, Type ownerType)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            return property.GetMeta(ownerType).IdProperty;
        }

        /// <summary>
        /// 获取对应实体的实体引用属性名
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static string GetRefEntityProperty(this IOEARefProperty property, object owner)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (owner == null) throw new ArgumentNullException("owner");

            return property.GetMeta(owner).RefEntityProperty;
        }

        /// <summary>
        /// 获取对应实体类型的实体引用属性名
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        public static string GetRefEntityProperty(this IOEARefProperty property, Type ownerType)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (ownerType == null) throw new ArgumentNullException("ownerType");

            return property.GetMeta(ownerType).RefEntityProperty;
        }
    }
}