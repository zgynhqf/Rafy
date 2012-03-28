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
using SimpleCsla.Validation;

namespace OEA
{
    /// <summary>
    /// 状态未
    /// </summary>
    public interface IDenpendentObject
    {
        /// <summary>
        /// 是否验证正确
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 所有的需要的条件
        /// </summary>
        ValidationRules ValidationRules { get; }

        /// <summary>
        /// 如果验证失败，这个对象的所有未满足的规则。
        /// </summary>
        BrokenRulesCollection BrokenRulesCollection { get; }

        /// <summary>
        /// 如果数据不符合规则，则抛出异常。
        /// </summary>
        void CheckRules();
    }

    /// <summary>
    /// 查询条件对象
    /// </summary>
    public interface IQueryObject : IDenpendentObject { }
}
