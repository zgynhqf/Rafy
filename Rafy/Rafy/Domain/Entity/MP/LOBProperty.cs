/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130526
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130526 14:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 大对象属性
    /// <remarks>大对象属性的查询，是使用懒加载的方式。</remarks>
    /// </summary>
    /// <typeparam name="TPropertyType">属性的类型，只支持两种类型：String，Byte[]</typeparam>
    public sealed class LOBProperty<TPropertyType> : Property<TPropertyType>, ILOBProperty
        where TPropertyType : class
    {
        internal LOBProperty(Type ownerType, Type declareType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, declareType, propertyName, defaultMeta) { }

        internal LOBProperty(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }

        public override PropertyCategory Category
        {
            get { return PropertyCategory.LOB; }
        }

        /// <summary>
        /// LOB属性的类型
        /// </summary>
        public LOBType LOBType { get; internal set; }
    }
}