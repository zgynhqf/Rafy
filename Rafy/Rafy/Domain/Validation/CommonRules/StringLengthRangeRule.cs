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
    /// 限制某字符串属性的字符长度范围的规则。
    /// 
    /// 如果字段串属性的值是 null 或空字段串，则验证规则会失效。此时可使用 <see cref="RequiredRule"/> 类型单独验证。
    /// </summary>
    public class StringLengthRangeRule : ValidationRule
    {
        public StringLengthRangeRule()
        {
            this.Min = int.MinValue;
            this.Max = int.MaxValue;
        }

        /// <summary>
        /// 最小长度。
        /// </summary>
        public int Min { get; set; }

        /// <summary>
        /// 最大长度。
        /// </summary>
        public int Max { get; set; }

        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var value = entity.GetProperty(e.Property) as string;
            if (!string.IsNullOrEmpty(value))
            {
                var min = this.Min;
                if (value.Length < min)
                {
                    if (this.MessageBuilder != null)
                    {
                        e.BrokenDescription = this.MessageBuilder(entity);
                    }
                    else
                    {
                        e.BrokenDescription = string.Format(
                            "{0} 不能低于 {1} 个字符。".Translate(),
                            e.DisplayProperty(), min
                            );
                    }
                }
                else
                {
                    var max = this.Max;
                    if (value.Length > max)
                    {
                        if (this.MessageBuilder != null)
                        {
                            e.BrokenDescription = this.MessageBuilder(entity);
                        }
                        else
                        {
                            e.BrokenDescription = string.Format(
                                "{0} 不能超过 {1} 个字符。".Translate(),
                                e.DisplayProperty(), max
                                );
                        }
                    }
                }
            }
        }
    }
}