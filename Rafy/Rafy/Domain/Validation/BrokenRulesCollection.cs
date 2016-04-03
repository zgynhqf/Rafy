/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 适配到 Entity、托管属性上。 胡庆访 20120330
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Rafy.ManagedProperty;
using Rafy.Serialization.Mobile;
using System.Collections.ObjectModel;
using System.Linq;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 违反的业务规则的集合。
    /// </summary>
    /// <remarks>
    /// This collection is readonly and can be safely made available
    /// to code outside the business object such as the UI. This allows
    /// external code, such as a UI, to display the list of broken rules
    /// to the user.
    /// </remarks>
    public class BrokenRulesCollection : Collection<BrokenRule>
    {
        internal void Add(IRule rule, RuleArgs args)
        {
            Remove(rule);
            BrokenRule item = new BrokenRule(rule, args);
            Add(item);
        }

        internal void Remove(IRule rule)
        {
            for (int index = 0; index < Count; index++)
            {
                if (this[index].Rule == rule)
                {
                    RemoveAt(index);
                    break;
                }
            }
        }

        #region 显示

        /// <summary>
        /// Returns the text of all broken rule descriptions, each
        /// separated by a <see cref="Environment.NewLine" />.
        /// </summary>
        /// <returns>The text of all broken rule descriptions.</returns>
        public override string ToString()
        {
            return this.ToString(Environment.NewLine);
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions
        /// for a specific severity, each
        /// separated by a <see cref="Environment.NewLine" />.
        /// </summary>
        /// <param name="level">The severity of rules to
        /// include in the result.</param>
        /// <returns>The text of all broken rule descriptions
        /// matching the specified severtiy.</returns>
        public string ToString(RuleLevel level)
        {
            return this.ToString(Environment.NewLine, level);
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions.
        /// </summary>
        /// <param name="separator">
        /// String to place between each broken rule description.
        /// </param>
        /// <returns>The text of all broken rule descriptions.</returns>
        public string ToString(string separator)
        {
            return string.Join(separator, this.Select(r => r.Description));
        }

        /// <summary>
        /// Returns the text of all broken rule descriptions
        /// for a specific severity.
        /// </summary>
        /// <param name="separator">
        /// String to place between each broken rule description.
        /// </param>
        /// <param name="level">The severity of rules to
        /// include in the result.</param>
        /// <returns>The text of all broken rule descriptions
        /// matching the specified severtiy.</returns>
        public string ToString(string separator, RuleLevel level)
        {
            return string.Join(Environment.NewLine, this.Where(r => r.Rule.Meta.Level == level).Select(r => r.Description));
        }

        #endregion
    }
}