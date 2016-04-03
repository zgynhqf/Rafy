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
using Rafy.ManagedProperty;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 验证规则及元数据。
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// 如果这个规则是适用到某个实体属性上，那么这个属性就表示这个实体的属性。
        /// </summary>
        IManagedProperty Property { get; }

        /// <summary>
        /// 获取用于表示规则名称。
        /// </summary>
        /// <remarks>
        /// 这个名称必须是唯一的，因为它会被用于在 BrokenRules 集合中标识唯一项。
        /// </remarks>
        string Key { get; }

        /// <summary>
        /// 规则对应的元数据。
        /// </summary>
        RuleMeta Meta { get; }

        /// <summary>
        /// 用于执行验证的规则逻辑。
        /// </summary>
        IValidationRule ValidationRule { get; }
    }

    /// <summary>
    /// 表示规则作用于实体的状态的作用范围
    /// </summary>
    [Flags]
    public enum EntityStatusScopes
    {
        /// <summary>
        /// 作用于数据的插入操作。
        /// </summary>
        Add = 1,
        /// <summary>
        /// 作用于数据的更新操作。
        /// </summary>
        Update = 2,
        /// <summary>
        /// 作用于数据的插入、更新操作。
        /// </summary>
        Delete = 4
    }
}