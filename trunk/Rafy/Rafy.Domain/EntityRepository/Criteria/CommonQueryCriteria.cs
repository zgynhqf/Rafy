/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140702
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140702 14:16
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Rafy;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 通用查询条件。使用该条件，可以解决大部分的查询需求。
    /// 用于表示使用 Or 连接多个组，每个组使用 And 进行连接查询的查询条件。
    /// 
    /// <example>
    /// 示例一，只有一个 And 组：
    /// var criteria = new CommonQueryCriteria
    /// {
    ///     new PropertyMatch(Entity.TreeIndexProperty, "1."),
    ///     new PropertyMatch(Entity.IdProperty, 1)
    /// };
    /// 表示以下条件：
    /// TreeIndex.Contains("1.") And Id == 1
    /// 
    /// 示例二，多个 And 组，使用 Or 进行连接：
    /// var criteria = new CommonQueryCriteria
    /// {
    ///     new PropertyMatch(Entity.TreeIndexProperty, "1."),
    ///     new PropertyMatch(Entity.IdProperty, 1),
    ///     new PropertyMatchCollection
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "1."),
    ///     },
    ///     new PropertyMatchCollection
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "2."),};
    ///     }
    /// };
    /// 表示：
    /// TreeIndex.Contains("1.") And Id == 1 
    ///     Or TreeIndex.StartWith("1.") And TreePId == "1." 
    ///     Or TreeIndex.StartWith("2.") And TreePId == "2."
    /// 
    /// 示例三，多个 Or 组，使用 And 进行连接：
    /// var criteria = new CommonQueryCriteria(BinaryOperator.And)
    /// {
    ///     new PropertyMatchCollection(BinaryOperator.Or)
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "1."),
    ///     },
    ///     new PropertyMatchCollection(BinaryOperator.Or)
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "2."),};
    ///     }
    /// };
    /// 表示：
    /// (TreeIndex.StartWith("1.") || TreePId == "1." )
    ///     And (TreeIndex.StartWith("2.") || TreePId == "2.")
    /// </example>
    /// </summary>
    [Serializable]
    public class CommonQueryCriteria : Criteria, IEnumerable<PropertyMatchCollection>, IEnumerable
    {
        private List<PropertyMatchCollection> _groups = new List<PropertyMatchCollection>();

        #region 构造器

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonQueryCriteria"/> class.
        /// </summary>
        public CommonQueryCriteria()
        {
            this.OrderByAscending = true;
            this.Concat = BinaryOperator.Or;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonQueryCriteria"/> class.
        /// </summary>
        /// <param name="concat">组与组之间的连接方式。</param>
        public CommonQueryCriteria(BinaryOperator concat)
        {
            this.OrderByAscending = true;
            this.Concat = concat;
        }

        #endregion

        /// <summary>
        /// 所有进行 Or 连接查询的组。每个组中的属性使用 And 进行连接。
        /// </summary>
        internal IList<PropertyMatchCollection> Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// 组与组之间的连接方式，默认使用 Or 连接。
        /// </summary>
        public BinaryOperator Concat { get; set; }

        /// <summary>
        /// 如果值为空（包括空字符串），则忽略该对比项。
        /// 默认为 false。
        /// </summary>
        public bool IgnoreNull { get; set; }

        /// <summary>
        /// 根据该属性进行排序。
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// 是否正序排列。默认为 true。
        /// </summary>
        public bool OrderByAscending { get; set; }

        #region 集合初始化器

        /// <summary>
        /// 添加一个 And 连接的属性匹配组。
        /// </summary>
        /// <param name="group"></param>
        public void Add(PropertyMatchCollection group)
        {
            _groups.Add(group);
        }

        /// <summary>
        /// 添加一个属性匹配条件到最后一个组中。
        /// </summary>
        /// <param name="propertyMatch">The property match.</param>
        public void Add(PropertyMatch propertyMatch)
        {
            var group = FindOrCreateGroup();
            group.Add(propertyMatch);
        }

        /// <summary>
        /// 添加一个属性匹配条件到最后一个组中。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="op">The op.</param>
        /// <param name="value">The value.</param>
        public void Add(IManagedProperty property, PropertyOperator op, object value)
        {
            this.Add(new PropertyMatch(property, op, value));
        }

        private PropertyMatchCollection FindOrCreateGroup()
        {
            if (_groups.Count > 0)
            {
                return _groups[_groups.Count - 1];
            }

            var group = new PropertyMatchCollection();
            _groups.Add(group);
            return group;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        IEnumerator<PropertyMatchCollection> IEnumerable<PropertyMatchCollection>.GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        #endregion

        //private static void Test()
        //{
        //    var criteria = new CommonQueryCriteria
        //    {
        //        new PropertyMatch(Entity.TreeIndexProperty, "1."),
        //        new PropertyMatch(Entity.IdProperty, 1),
        //        new PropertyMatchCollection
        //        {
        //            new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
        //            new PropertyMatch(Entity.TreePIdProperty, "1."),
        //        },
        //        new PropertyMatchCollection
        //        {
        //            new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
        //            new PropertyMatch(Entity.TreePIdProperty, "2."),
        //        }
        //    };
        //}
    }

    [Serializable]
    public class PropertyMatchCollection : Collection<PropertyMatch>
    {
        #region 构造器

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMatchCollection"/> class.
        /// </summary>
        public PropertyMatchCollection()
        {
            this.Concat = BinaryOperator.And;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMatchCollection"/> class.
        /// </summary>
        /// <param name="concat">属性条件之间的连接符。</param>
        public PropertyMatchCollection(BinaryOperator concat)
        {
            this.Concat = concat;
        }

        #endregion

        /// <summary>
        /// 属性条件之间的连接符，默认是 And。
        /// </summary>
        public BinaryOperator Concat { get; set; }
    }

    /// <summary>
    /// 属性的对比条件
    /// </summary>
    [Serializable]
    public class PropertyMatch
    {
        #region 构造器

        public PropertyMatch(IManagedProperty property, PropertyOperator op, object value)
            : this(property.Name, op, value) { }

        public PropertyMatch(IManagedProperty property, object value)
            : this(property.Name, value) { }

        public PropertyMatch(string property, object value)
        {
            this.PropertyName = property;
            this.Operator = value is string ? PropertyOperator.Contains : PropertyOperator.Equal;
            this.Value = value;
        }

        public PropertyMatch(string property, PropertyOperator op, object value)
        {
            this.PropertyName = property;
            this.Operator = value is string ? PropertyOperator.Contains : PropertyOperator.Equal;
            this.Value = value;
        }

        #endregion

        /// <summary>
        /// 属性名称。
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// 属性操作符。
        /// </summary>
        public PropertyOperator Operator { get; private set; }

        /// <summary>
        /// 对比的值。
        /// </summary>
        public object Value { get; private set; }
    }
}