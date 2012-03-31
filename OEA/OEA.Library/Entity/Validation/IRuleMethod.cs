/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120330
 * 
*******************************************************/

using System;

namespace OEA.Library.Validation
{
    /// <summary>
    /// Tracks all information for a rule.
    /// </summary>
    public interface IRuleMethod
    {
        /// <summary>
        /// Gets the name of the rule.
        /// </summary>
        /// <remarks>
        /// The rule's name must be unique and is used
        /// to identify a broken rule in the BrokenRules
        /// collection.
        /// </remarks>
        string RuleLabel { get; }

        /// <summary>
        /// Returns the name of the field, property or column
        /// to which the rule applies.
        /// </summary>
        RuleArgs RuleArgs { get; }

        /// <summary>
        /// Gets the priority of the rule method.
        /// </summary>
        /// <value>The priority value.</value>
        /// <remarks>
        /// Priorities are processed in descending
        /// order, so priority 0 is processed
        /// before priority 1, etc.</remarks>
        int Priority { get; }

        /// <summary>
        /// Invokes the rule to validate the data.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the data is valid, 
        /// <see langword="false" /> if the data is invalid.
        /// </returns>
        void Check(Entity target);
    }
}