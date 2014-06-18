/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140313
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140313 11:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 验证规则的声明器。
    /// </summary>
    public interface IValidationDeclarer
    {
        /// <summary>
        /// 获取当前已经声明的规则的个数。
        /// </summary>
        int RulesCount { get; }

        /// <summary>
        /// 为整个实体添加一个业务验证规则。
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        void AddRule(RuleHandler handler,
           object parameters = null, RuleLevel level = RuleLevel.Error, int priority = 0
           );

        /// <summary>
        /// 为某个属性添加一个业务验证规则。
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="level">The level.</param>
        /// <param name="priority">The priority.</param>
        void AddRule(IManagedProperty property, RuleHandler handler,
           object parameters = null, RuleLevel level = RuleLevel.Error, int priority = 0
           );


        /// <summary>
        /// 清空所有规则。
        /// </summary>
        void ClearRules();

        /// <summary>
        /// 清空指定属性对应的规则。
        /// </summary>
        /// <param name="property"></param>
        void ClearRules(IManagedProperty property);
    }
}
