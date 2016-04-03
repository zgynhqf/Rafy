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
    /// 示例二，多个 Or 组，使用 And 进行连接：
    /// var criteria = new CommonQueryCriteria
    /// {
    ///     new PropertyMatchGroup
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "1."),
    ///     },
    ///     new PropertyMatchGroup
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "2."),};
    ///     }
    /// };
    /// 表示：
    /// (TreeIndex.StartWith("1.") || TreePId == "1." )
    ///     And (TreeIndex.StartWith("2.") || TreePId == "2.")
    /// 
    /// 示例三，多个 And 组，使用 Or 进行连接：
    /// var criteria = new CommonQueryCriteria(BinaryOperator.Or)
    /// {
    ///     new PropertyMatch(Entity.TreeIndexProperty, "1."),
    ///     new PropertyMatch(Entity.IdProperty, 1),
    ///     new PropertyMatchGroup(BinaryOperator.And)
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "1."),
    ///     },
    ///     new PropertyMatchGroup(BinaryOperator.And)
    ///     {
    ///         new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
    ///         new PropertyMatch(Entity.TreePIdProperty, "2."),};
    ///     }
    /// };
    /// 表示：
    /// TreeIndex.Contains("1.") Or Id == 1 
    ///     Or TreeIndex.StartWith("1.") And TreePId == "1." 
    ///     Or TreeIndex.StartWith("2.") And TreePId == "2."
    /// </example>
    /// </summary>
    [Serializable]
    public class CommonQueryCriteria : Criteria, IEnumerable<IPropertyMatchGroup>, IEnumerable
    {
        private List<IPropertyMatchGroup> _groups = new List<IPropertyMatchGroup>();

        #region 构造器

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonQueryCriteria"/> class.
        /// </summary>
        public CommonQueryCriteria()
        {
            this.OrderByAscending = true;
            this.Concat = BinaryOperator.And;
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

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected CommonQueryCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        /// <summary>
        /// 所有进行 Or 连接查询的组。每个组中的属性使用 And 进行连接。
        /// </summary>
        internal IList<IPropertyMatchGroup> Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// 组与组之间的连接方式，默认使用 And 连接。
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

        /// <summary>
        /// 返回当前已经拥有的组的个数。
        /// </summary>
        public int GroupsCount
        {
            get { return _groups.Count; }
        }

        #region 集合初始化器

        /// <summary>
        /// 添加一个 And 连接的属性匹配组。
        /// </summary>
        /// <param name="group"></param>
        public void Add(PropertyMatchGroup group)
        {
            _groups.Add(group);
        }

        /// <summary>
        /// 添加一个属性匹配条件为一个单独的组。
        /// </summary>
        /// <param name="propertyMatch">The property match.</param>
        public void Add(PropertyMatch propertyMatch)
        {
            _groups.Add(new PropertyMatchGroup() { propertyMatch });
        }

        /// <summary>
        /// 添加一个属性匹配条件到本组中。使用 Equal 进行对比。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public void Add(IManagedProperty property, object value)
        {
            this.Add(property, PropertyOperator.Equal, value);
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

        /// <summary>
        /// 添加一个属性匹配条件到最后一个组中。
        /// 如果此时不没有任何一个组，则会自动创建一个新组。
        /// </summary>
        /// <param name="propertyMatch">The property match.</param>
        public void AddToLastGroup(PropertyMatch propertyMatch)
        {
            var group = FindOrCreateGroup();
            this.AddToGroup(group, propertyMatch);
        }

        /// <summary>
        /// 添加指定的属性匹配到组集合中。
        /// </summary>
        /// <param name="group"></param>
        /// <param name="item"></param>
        public void AddToGroup(IPropertyMatchGroup group, PropertyMatch item)
        {
            var realGroup = group as PropertyMatchGroup;

            //如果 group 不是一个真正的组，那么它就是一个 PropertyMatch。
            //这时再添加新的元素到这个组中，需要把它升级为一个真正的组集合。
            if (realGroup == null)
            {
                var index = _groups.IndexOf(group);
                if (index < 0)
                {
                    throw new InvalidOperationException("group 没有在本条件中，不能为其添加元素。请先将 group 添加到本条件中。");
                }
                realGroup = new PropertyMatchGroup { group as PropertyMatch };
                _groups[index] = realGroup;
            }

            realGroup.Add(item);
        }

        private IPropertyMatchGroup FindOrCreateGroup()
        {
            if (_groups.Count > 0)
            {
                return _groups[_groups.Count - 1];
            }

            var group = new PropertyMatchGroup();
            _groups.Add(group);
            return group;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _groups.GetEnumerator();
        }

        IEnumerator<IPropertyMatchGroup> IEnumerable<IPropertyMatchGroup>.GetEnumerator()
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
        //        new PropertyMatchGroup
        //        {
        //            new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "1."),
        //            new PropertyMatch(Entity.TreePIdProperty, "1."),
        //        },
        //        new PropertyMatchGroup
        //        {
        //            new PropertyMatch(Entity.TreeIndexProperty, PropertyOperator.StartWith, "2."),
        //            new PropertyMatch(Entity.TreePIdProperty, "2."),
        //        }
        //    };
        //}
    }

    /// <summary>
    /// 一组属性匹配。
    /// </summary>
    [Serializable]
    public class PropertyMatchGroup : Collection<PropertyMatch>, IPropertyMatchGroup
    {
        #region 构造器

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMatchGroup"/> class.
        /// </summary>
        public PropertyMatchGroup()
        {
            this.Concat = BinaryOperator.Or;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMatchGroup"/> class.
        /// </summary>
        /// <param name="concat">属性条件之间的连接符。</param>
        public PropertyMatchGroup(BinaryOperator concat)
        {
            this.Concat = concat;
        }

        #endregion

        /// <summary>
        /// 属性条件之间的连接符，默认是 Or。
        /// </summary>
        public BinaryOperator Concat { get; set; }

        /// <summary>
        /// 添加一个属性匹配条件到本组中。使用 Equal 进行对比。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public void Add(IManagedProperty property, object value)
        {
            this.Add(property, PropertyOperator.Equal, value);
        }

        /// <summary>
        /// 添加一个属性匹配条件到本组中。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="op">The op.</param>
        /// <param name="value">The value.</param>
        public void Add(IManagedProperty property, PropertyOperator op, object value)
        {
            this.Add(new PropertyMatch(property, op, value));
        }
    }

    /// <summary>
    /// 属性的对比条件
    /// </summary>
    [Serializable]
    public class PropertyMatch : IPropertyMatchGroup
    {
        #region 构造器

        public PropertyMatch(IManagedProperty property, PropertyOperator op, object value)
            : this(property.Name, op, value) { }

        public PropertyMatch(IManagedProperty property, object value)
            : this(property.Name, value) { }

        public PropertyMatch(string property, object value)
        {
            this.PropertyName = property;
            this.Operator = PropertyOperator.Equal;
            this.Value = value;
        }

        public PropertyMatch(string property, PropertyOperator op, object value)
        {
            this.PropertyName = property;
            this.Operator = op;
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

        #region IPropertyMatchGroup

        int IPropertyMatchGroup.Count
        {
            get { return 1; }
        }

        BinaryOperator IPropertyMatchGroup.Concat
        {
            get { return BinaryOperator.And; }
        }

        PropertyMatch IPropertyMatchGroup.this[int index]
        {
            get
            {
                if (index != 0) { throw new ArgumentOutOfRangeException("index"); }
                return this;
            }
        }

        IEnumerator<PropertyMatch> IEnumerable<PropertyMatch>.GetEnumerator()
        {
            return new SingletonEnumerator { Instance = this };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SingletonEnumerator { Instance = this };
        }

        private class SingletonEnumerator : IEnumerator<PropertyMatch>
        {
            private bool _canReturn = true;
            public PropertyMatch Instance;

            public PropertyMatch Current
            {
                get { return Instance; }
            }

            public void Dispose() { }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (_canReturn)
                {
                    _canReturn = false;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                _canReturn = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// 一组属性匹配。
    /// </summary>
    public interface IPropertyMatchGroup : IEnumerable<PropertyMatch>
    {
        /// <summary>
        /// 获取指定索引的元素。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        PropertyMatch this[int index] { get; }

        /// <summary>
        /// 有多个属性属性在其中。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 属性条件之间的连接符，默认是 Or。
        /// </summary>
        BinaryOperator Concat { get; }
    }
}