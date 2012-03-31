/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library.Validation;
using OEA.ManagedProperty;

namespace OEA.Library
{
    /// <summary>
    /// 状态未确实的对象
    /// </summary>
    public interface IValidatable
    {
        ///// <summary>
        ///// 是否验证正确
        ///// </summary>
        //bool IsValid { get; }

        ///// <summary>
        ///// 所有的需要的条件
        ///// </summary>
        //ValidationRules ValidationRules { get; }

        ///// <summary>
        ///// 如果验证失败，这个对象的所有未满足的规则。
        ///// </summary>
        //BrokenRulesCollection BrokenRulesCollection { get; }

        /// <summary>
        /// 检查整个实体对象是否满足规则
        /// </summary>
        BrokenRulesCollection Validate();

        /// <summary>
        /// 检查某个属性是否满足规则
        /// </summary>
        /// <param name="property">托管属性</param>
        BrokenRulesCollection Validate(IManagedProperty property);
    }
}
