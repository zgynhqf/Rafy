/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140719
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140719 13:22
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
    /// <summary>
    /// 限制某数值类型属性正数限制规则。
    /// </summary>
    public class PositiveNumberRule : ValidationRule
    {
        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var value = Convert.ToDouble(entity.GetProperty(e.Property));

            if (value <= 0)
            {
                if (this.MessageBuilder != null)
                {
                    e.BrokenDescription = this.MessageBuilder(entity);
                }
                else
                {
                    e.BrokenDescription = string.Format("{0} 需要是正数。".Translate(), e.DisplayProperty());
                }
            }
        }
    }
}
