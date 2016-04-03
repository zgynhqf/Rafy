/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 12:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 验证规则
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// 此属性指示本规则中是否需要连接数据源。
        /// </summary>
        bool ConnectToDataSource { get; }

        /// <summary>
        /// 对某个实体进行验证。
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="e">The RuleArgs.</param>
        void Validate(ManagedPropertyObject entity, RuleArgs e);
    }
}