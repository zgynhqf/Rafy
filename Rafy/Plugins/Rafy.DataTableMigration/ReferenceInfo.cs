using System;
using System.Collections.Generic;
using Rafy.Domain;

namespace Rafy.DataTableMigration
{
    /// <summary>
    /// 表示实体的引用信息。
    /// </summary>
    internal class ReferenceInfo : IComparable<ReferenceInfo>
    {
        /// <summary>
        /// 初始化 <see cref="ReferenceInfo"/> 类的新实例。
        /// </summary>
        public ReferenceInfo()
        {
            this.ReferencePropertyTypes = new List<Type>();
        }

        /// <summary>
        /// 获取或设置引用次数。
        /// </summary>
        public int ReferenceTimes { get; set; }

        /// <summary>
        /// 获取或设置引用属性的类型的集合。
        /// </summary>
        public List<Type> ReferencePropertyTypes { get; }

        /// <summary>
        /// 获取或设置当前引用实体的仓库。
        /// </summary>
        public EntityRepository Repository { get; set; }

        /// <summary>
        /// 添加引用属性的类型到集合中。
        /// </summary>
        /// <param name="type">引用属性的类型。</param>
        public void AddReferenceType(Type type)
        {
            if (this.ReferencePropertyTypes.Contains(type))
            {
                return;
            }

            this.ReferencePropertyTypes.Add(type);
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object. </summary>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="other" /> in the sort order.  Zero This instance occurs in the same position in the sort order as <paramref name="other" />. Greater than zero This instance follows <paramref name="other" /> in the sort order. </returns>
        /// <param name="other">An object to compare with this instance. </param>
        public int CompareTo(ReferenceInfo other)
        {
            if (other == null)
            {
                return -1;
            }

            if (other.ReferenceTimes == this.ReferenceTimes)
            {
                return 0;
            }

            if (other.ReferenceTimes < this.ReferenceTimes)
            {
                return 1;
            }

            return -1;
        }
    }
}