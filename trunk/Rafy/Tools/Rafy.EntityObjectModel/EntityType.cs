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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 表示一个实体类型的对象模型
    /// </summary>
    public class EntityType : EOMObject
    {
        public EntityType()
        {
            this.ValueProperties = new ValuePropertyCollection(this);
            this.References = new ReferenceCollection(this);
            this.Children = new ChildCollection(this);
        }

        public string Name { get; set; }

        /// <summary>
        /// 包含命名空间的类型名。
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 当前的实体类型是否为一个聚合根。
        /// </summary>
        public bool IsAggtRoot { get; set; }

        /// <summary>
        /// 本实体类型的基类。
        /// </summary>
        public EntityType BaseType { get; set; }

        /// <summary>
        /// 定义在此实体类型中的值属性。
        /// </summary>
        public ValuePropertyCollection ValueProperties { get; private set; }

        /// <summary>
        /// 定义在此实体类型中的引用属性。
        /// </summary>
        public ReferenceCollection References { get; private set; }

        /// <summary>
        /// 定义在此实体类型中的实体列表子属性。
        /// </summary>
        public ChildCollection Children { get; private set; }

        ///// <summary>
        ///// 遍历所有的值属性，包括继承下来的属性。
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<ValueProperty> AllValueProperties()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        ///// <summary>
        ///// 遍历所有的引用属性，包括继承下来的属性。
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<Reference> AllReferences()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        ///// <summary>
        ///// 遍历所有的子属性，包括继承下来的属性。
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<Child> AllChildren()
        //{
        //    throw new NotImplementedException();//huqf
        //}

        public override string ToString()
        {
            return this.FullName;
        }
    }
}