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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 限制某数值类型属性最小的数据限制规则。
    /// 注意：此规则不对空字符串作判断。如果需要值非空的约束，请使用 RequiredRule。
    /// </summary>
    public class RegexMatchRule : ValidationRule
    {
        /// <summary>
        /// 最小值。
        /// </summary>
        public Regex Regex { get; set; }

        /// <summary>
        /// 正则的名称。
        /// </summary>
        public string RegexLabel { get; set; }

        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var value = (string)entity.GetProperty(e.Property) ?? string.Empty;

            if (!string.IsNullOrEmpty(value))
            {
                if (!this.Regex.IsMatch(value))
                {
                    if (this.MessageBuilder != null)
                    {
                        e.BrokenDescription = this.MessageBuilder(entity);
                    }
                    else
                    {
                        e.BrokenDescription = string.Format(
                            "{0} 必须是 {1}。".Translate(),
                            e.DisplayProperty(),
                            this.RegexLabel.Translate()
                            );
                    }
                }
            }
        }
    }
}