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
using OEA.ManagedProperty;
using OEA;

namespace OEA.Library.Validation
{
    /// <summary>
    /// Implements common business rules.
    /// </summary>
    public static class CommonRules
    {
        /// <summary>
        /// 不为空(null)。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void Required(Entity target, RuleArgs e)
        {
            var property = e.Property;

            bool isNull = false;

            if (property is IRefProperty)
            {
                var lazyRef = target.GetLazyRef(property as IRefProperty);
                isNull = lazyRef.NullableId == null;
            }
            else
            {
                var value = target.GetProperty(property);
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
                e.BrokenDescription = string.Format("{0} 并没有填写。", e.GetPropertyDisplay());
            }
        }

        /// <summary>
        /// 最大的字符长度。
        /// 
        /// 注意，此验证需要参数：
        /// int MaxLength
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void StringMaxLength(Entity target, RuleArgs e)
        {
            var max = e.TryGetCustomParams<int>("MaxLength");
            var value = target.GetProperty(e.Property) as string;

            if (!string.IsNullOrEmpty(value) && value.Length > max)
            {
                e.BrokenDescription = string.Format("{0} 不能超过 {1} 个字符。", e.GetPropertyDisplay(), max);
            }
        }

        /// <summary>
        /// 最短的字符长度。
        /// 
        /// 注意，此验证需要参数：
        /// int MinLength
        /// </remarks>
        public static void StringMinLength(Entity target, RuleArgs e)
        {
            var min = e.TryGetCustomParams<int>("MaxLength");
            var value = target.GetProperty(e.Property) as string;

            if (!string.IsNullOrEmpty(value) && value.Length < min)
            {
                e.BrokenDescription = string.Format("{0} 不能低于 {1} 个字符。", e.GetPropertyDisplay(), min);
            }
        }

        /// <summary>
        /// 最大的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// double MaxValue
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void MaxValue(Entity target, RuleArgs e)
        {
            var max = e.TryGetCustomParams<double>("MaxValue");
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value > max)
            {
                e.BrokenDescription = string.Format("{0} 不能超过 {1}。", e.GetPropertyDisplay(), max);
            }
        }

        /// <summary>
        /// 最小的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// double MinValue
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void MinValue(Entity target, RuleArgs e)
        {
            var min = e.TryGetCustomParams<double>("MinValue");
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value < min)
            {
                e.BrokenDescription = string.Format("{0} 不能低于 {1}。", e.GetPropertyDisplay(), min);
            }
        }

        /// <summary>
        /// 正数限制规则。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void Positive(Entity target, RuleArgs e)
        {
            var value = Convert.ToDouble(target.GetProperty(e.Property));

            if (value <= 0)
            {
                e.BrokenDescription = string.Format("{0} 需要是正数。", e.GetPropertyDisplay());
            }
        }

        /// <summary>
        /// 最小的数据限制规则。
        /// 
        /// 注意，此验证需要参数：
        /// Regex Regex
        /// string RegexLabel
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        public static void RegexMatch(Entity target, RuleArgs e)
        {
            Regex re = e.TryGetCustomParams<Regex>("Regex");
            var value = (string)target.GetProperty(e.Property) ?? string.Empty;
            if (!re.IsMatch(value))
            {
                var regexLabel = e.TryGetCustomParams<string>("RegexLabel");
                e.BrokenDescription = string.Format("{0} 必须是 {1}。", e.GetPropertyDisplay(), regexLabel);
            }
        }
    }
}