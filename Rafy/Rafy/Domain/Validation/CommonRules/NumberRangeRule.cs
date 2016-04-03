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
    /// 限制某数值类型属性的数据范围的规则。
    /// </summary>
    public class NumberRangeRule : ValidationRule
    {
        public NumberRangeRule()
        {
            this.Min = double.MinValue;
            this.Max = double.MaxValue;
        }

        /// <summary>
        /// 最小值。
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// 最小值。
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// 设置此属性可以自定义要显示的错误信息。
        /// </summary>
        public Func<Entity, string> MessageBuilder { get; set; }

        protected override void Validate(Entity entity, RuleArgs e)
        {
            var value = Convert.ToDouble(entity.GetProperty(e.Property));

            var min = this.Min;
            if (value < min)
            {
                if (this.MessageBuilder != null)
                {
                    e.BrokenDescription = this.MessageBuilder(entity);
                }
                else
                {
                    e.BrokenDescription = string.Format("{0} 不能低于 {1}。".Translate(), e.DisplayProperty(), min);
                }
            }
            else
            {
                var max = this.Max;
                if (value > max)
                {
                    if (this.MessageBuilder != null)
                    {
                        e.BrokenDescription = this.MessageBuilder(entity);
                    }
                    else
                    {
                        e.BrokenDescription = string.Format("{0} 不能超过 {1}。".Translate(), e.DisplayProperty(), max);
                    }
                }
            }
        }
    }
}
