/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120331
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120331
 * 
*******************************************************/

using System;
using System.Text.RegularExpressions;
using System.Reflection;
using Rafy.ManagedProperty;
using Rafy;
using Rafy.MetaModel;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 一般性的业务规则。
    /// </summary>
    public static class CommonRules
    {
        /// <summary>
        /// 约束某属性不能为空值（null）。
        /// 字符串还应该不能为空字符串。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void Required(ManagedPropertyObject target, RuleArgs e)
        {
            var entity = target as Entity;
            var property = e.Property;

            bool isNull = false;

            if (property is IRefProperty)
            {
                var id = entity.GetRefNullableId((property as IRefProperty).RefIdProperty);
                isNull = id == null;
            }
            else
            {
                var value = entity.GetProperty(property);
                if (property.PropertyType == typeof(string))
                {
                    isNull = string.IsNullOrEmpty(value as string);
                }
                else
                {
                    isNull = value == null;
                }
            }

            if (isNull)
            {
                e.BrokenDescription = string.Format("{0} 里没有输入值。".Translate(), e.DisplayProperty());
            }
        }

        /// <summary>
        /// 限制某字符串属性最大的字符长度。
        /// 
        /// 注意，此验证需要参数：
        /// int MaxLength
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void StringMaxLength(ManagedPropertyObject target, RuleArgs e)
        {
            var max = e.GetPropertyOrDefault<int>("MaxLength");
            var value = target.GetProperty(e.Property) as string;

            if (!string.IsNullOrEmpty(value) && value.Length > max)
            {
                e.BrokenDescription = string.Format(
                    "{0} 不能超过 {1} 个字符。".Translate(),
                    e.DisplayProperty(), max
                    );
            }
        }

        /// <summary>
        /// 限制某字符串属性最短的字符长度。
        /// 
        /// 注意，此验证需要参数：
        /// int MinLength
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="e">The decimal.</param>
        public static void StringMinLength(ManagedPropertyObject target, RuleArgs e)
        {
            var min = e.GetPropertyOrDefault<int>("MaxLength");
            var value = target.GetProperty(e.Property) as string;

            if (!string.IsNullOrEmpty(value) && value.Length < min)
            {
                e.BrokenDescription = string.Format(
                    "{0} 不能低于 {1} 个字符。".Translate(),
                    e.DisplayProperty(), min
                    );
            }
        }

        /// <summary>
        /// 限制某数值类型属性最大的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// double MaxValue
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void MaxValue(ManagedPropertyObject target, RuleArgs e)
        {
            var max = e.GetPropertyOrDefault<double>("MaxValue");
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value > max)
            {
                e.BrokenDescription = string.Format("{0} 不能超过 {1}。".Translate(),
                    e.DisplayProperty(), max);
            }
        }

        /// <summary>
        /// 限制某数值类型属性最小的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// double MinValue
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void MinValue(ManagedPropertyObject target, RuleArgs e)
        {
            var min = e.GetPropertyOrDefault<double>("MinValue");
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value < min)
            {
                e.BrokenDescription = string.Format("{0} 不能低于 {1}。".Translate(), e.DisplayProperty(), min);
            }
        }

        /// <summary>
        /// 限制某数值类型属性正数限制规则。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void Positive(ManagedPropertyObject target, RuleArgs e)
        {
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value <= 0)
            {
                e.BrokenDescription = string.Format("{0} 需要是正数。".Translate(), e.DisplayProperty());
            }
        }

        /// <summary>
        /// 限制某数值类型属性最小的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// Regex Regex
        /// string RegexLabel
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void RegexMatch(ManagedPropertyObject target, RuleArgs e)
        {
            Regex re = e.GetPropertyOrDefault<Regex>("Regex");
            var value = (string)target.GetProperty(e.Property) ?? string.Empty;
            if (!re.IsMatch(value))
            {
                var regexLabel = e.GetPropertyOrDefault<string>("RegexLabel");
                e.BrokenDescription = string.Format(
                    "{0} 必须是 {1}。".Translate(),
                    e.DisplayProperty(),
                    regexLabel.Translate()
                    );
            }
        }
    }
}