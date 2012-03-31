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
    /// Delegate that defines the method signature for all rule handler methods.
    /// </summary>
    /// <param name="target">
    /// Object containing the data to be validated.
    /// </param>
    /// <param name="e">
    /// Parameter used to pass information to and from
    /// the rule method.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the rule was satisfied.
    /// </returns>
    /// <remarks>
    /// <para>
    /// When implementing a rule handler, you must conform to the method signature
    /// defined by this delegate. You should also apply the Description attribute
    /// to your method to provide a meaningful description for your rule.
    /// </para><para>
    /// The method implementing the rule must return 
    /// <see langword="true"/> if the data is valid and
    /// return <see langword="false"/> if the data is invalid.
    /// </para>
    /// </remarks>
    public delegate void RuleHandler(Entity target, RuleArgs e);
}