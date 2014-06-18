/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 组合子属性。
    /// </summary>
    public class Child : Property, IProperty
    {
        /// <summary>
        /// 聚合子类型在父类型中的属性名。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 子类型的列表类型名
        /// </summary>
        public string ListTypeFullName { get; set; }

        /// <summary>
        /// 聚合子的实体类型
        /// </summary>
        public EntityType ChildEntityType { get; set; }

        internal override string GetName()
        {
            return this.Name;
        }

        internal override string GetPropertyType()
        {
            return this.ListTypeFullName;
        }

        string IProperty.PropertyType
        {
            get { return this.ListTypeFullName; }
        }
    }
}