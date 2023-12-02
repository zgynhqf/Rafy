using Rafy.Domain;
using Rafy.ManagedProperty;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// IRefProperty 的帮助类。
    /// </summary>
    public class RefPropertyHelper
    {
        /// <summary>
        /// 返回属性对应的引用属性。
        /// </summary>
        /// <param name="property">传入的可以是引用键属性、引用实体属性。</param>
        /// <returns></returns>
        public static IRefProperty Find(IManagedProperty property)
        {
            if (property == null) return null;
            return property as IRefProperty ?? (property as IManagedPropertyInternal).RefEntityProperty;
        }

        /// <summary>
        /// 判断指定的属性是否是一个引用键属性。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsRefKeyProperty(IManagedProperty property)
        {
            return IsRefKeyProperty(property, out var refProperty);
        }

        /// <summary>
        /// 判断指定的属性是否是一个引用键属性。
        /// 目前，一般属性、<see cref="IRefIdProperty"/> 都可以作为引用键属性。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="refEntityProperty">如果是，则返回对应的引用实体属性。</param>
        /// <returns></returns>
        public static bool IsRefKeyProperty(IManagedProperty property, out IRefEntityProperty refEntityProperty)
        {
            refEntityProperty = (property as IManagedPropertyInternal).RefEntityProperty;
            return refEntityProperty != null;
        }
    }
}