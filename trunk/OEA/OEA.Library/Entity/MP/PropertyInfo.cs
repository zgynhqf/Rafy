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

namespace OEA.Library
{
    /// <summary>
    /// OEA 中所有实体的属性标记都使用这个类或者这个类的子类
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class Property<TPropertyType> : ManagedProperty<TPropertyType>
    {
        public Property(Type ownerType, string propertyName, ManagedPropertyMetadata<TPropertyType> defaultMeta) : base(ownerType, propertyName, defaultMeta) { }
    }
}