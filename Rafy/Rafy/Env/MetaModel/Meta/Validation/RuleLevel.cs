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

namespace Rafy.MetaModel
{
    /// <summary>
    /// Values for validation rule severities.
    /// </summary>
    public enum RuleLevel
    {
        /// <summary>
        /// Represents a serious
        /// business rule violation that
        /// should cause an object to
        /// be considered invalid.
        /// </summary>
        Error = 0,

        /// <summary>
        /// Represents a business rule
        /// violation that should be
        /// displayed to the user, but which
        /// should not make an object be
        /// invalid.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Represents a business rule
        /// result that should be displayed
        /// to the user, but which is less
        /// severe than a warning.
        /// </summary>
        Information = 2
    }
}
