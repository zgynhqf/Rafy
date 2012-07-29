/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120605 13:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120605 13:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA.ORM
{
    /// <summary>
    /// 某个外键引用属性对应的信息
    /// </summary>
    internal class RefTableProperty
    {
        public RefTableProperty(IRefProperty refProperty, Type propertyOwnerType = null)
        {
            if (propertyOwnerType == null) propertyOwnerType = refProperty.OwnerType;

            this.PropertyOwnerType = propertyOwnerType;
            this.RefProperty = refProperty;
            var mainTable = DbTableHost.TableFor(propertyOwnerType);
            var refTable = DbTableHost.TableFor(refProperty.RefEntityType);

            this.OwnerTable = mainTable;
            this.RefTable = refTable;
            this.FKName = mainTable.Translate(refProperty);
        }

        /// <summary>
        /// 属性的声明类型
        /// 
        /// 可以在构造函数中显式指定。
        /// </summary>
        internal Type PropertyOwnerType { get; private set; }

        /// <summary>
        /// 表示关系的实体引用属性
        /// </summary>
        internal IRefProperty RefProperty { get; private set; }

        /// <summary>
        /// A.B 中的 A，主表。
        /// </summary>
        internal DbTable OwnerTable { get; private set; }

        /// <summary>
        /// A.B 中的 B。
        /// 引用实体对应的表信息
        /// </summary>
        internal DbTable RefTable { get; private set; }

        /// <summary>
        /// 该实体属性对应的数据库外键名
        /// </summary>
        internal string FKName { get; private set; }
    }
}