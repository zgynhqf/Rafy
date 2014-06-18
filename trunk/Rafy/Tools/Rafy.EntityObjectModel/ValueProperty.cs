/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:26
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 值属性
    /// </summary>
    public class ValueProperty : Property, IProperty
    {
        /// <summary>
        /// 属性名。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 属性类型。
        /// </summary>
        public ValuePropertyType PropertyType { get; set; }

        /// <summary>
        /// 如果此属性是一个枚举类型的属性，则这个属性表示对应的枚举类型。
        /// </summary>
        public EnumType EnumType { get; set; }

        /// <summary>
        /// 此值对象是否可空
        /// </summary>
        public bool Nullable { get; set; }

        internal override string GetName()
        {
            return this.Name;
        }

        internal override string GetPropertyType()
        {
            return this.PropertyType.ToString();
        }
    }
}