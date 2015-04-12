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
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 某个外键引用属性对应的信息
    /// </summary>
    internal class RefTableProperty
    {
        public RefTableProperty(IRefIdProperty refProperty, Type propertyOwner)
        {
            this.PropertyOwner = propertyOwner;
            this.RefProperty = refProperty;
            var mainTable = RdbTableFinder.TableFor(propertyOwner);
            var refTable = RdbTableFinder.TableFor(refProperty.RefEntityType);

            this.OwnerTable = mainTable;
            this.RefTable = refTable;
            this.FKName = mainTable.Translate(refProperty);
        }

        /// <summary>
        /// 是否需要在查询时，同时查询出该实体的信息。
        /// </summary>
        internal JoinRefType JoinRefType;

        /// <summary>
        /// 属性的声明类型
        /// 
        /// 可以在构造函数中显式指定。
        /// </summary>
        internal Type PropertyOwner { get; private set; }

        /// <summary>
        /// 表示关系的实体引用属性
        /// </summary>
        internal IRefIdProperty RefProperty { get; private set; }

        /// <summary>
        /// A.B 中的 A，主表。
        /// </summary>
        internal RdbTable OwnerTable { get; private set; }

        /// <summary>
        /// A.B 中的 B。
        /// 引用实体对应的表信息
        /// </summary>
        internal RdbTable RefTable { get; private set; }

        /// <summary>
        /// 该实体属性对应的数据库外键名
        /// </summary>
        internal string FKName { get; private set; }
    }
}