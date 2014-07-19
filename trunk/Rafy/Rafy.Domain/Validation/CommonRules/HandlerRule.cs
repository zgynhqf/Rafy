/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 21:31
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    internal class HandlerRule : ValidationRule
    {
        public RuleHandler Handler { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            this.Handler(entity, e);
        }

        public string GetKeyString()
        {
            return string.Format(@"rule://{0}/{1}",
                Handler.Method.DeclaringType.FullName,
                Handler.Method.Name
                );
        }
    }
}
