using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Rafy.MetaModel.Attributes;
using Rafy;
using Rafy.Reflection;
using System.ComponentModel;

namespace Rafy.Utils
{
    /// <summary>
    /// 枚举值的逻辑视图
    /// </summary>
    public class EnumViewModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="value">枚举值</param>
        public EnumViewModel(Enum value)
        {
            this.EnumValue = value;
            this.Label = EnumToLabel(value);
            this.TranslatedLabel = this.Label.Translate();
        }

        private EnumViewModel(Enum value, string label)
        {
            this.EnumValue = value;
            this.Label = label;
            this.TranslatedLabel = this.Label.Translate();
        }

        /// <summary>
        /// 枚举值
        /// </summary>
        public Enum EnumValue { get; private set; }

        /// <summary>
        /// 枚举值的显示值
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// 翻译后的枚举值的显示值
        /// </summary>
        public string TranslatedLabel { get; private set; }

        /// <summary>
        /// EnumPropertyEditor 和其它地方中使用了这个方法来对比实体。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is EnumViewModel)
                return EnumValue.Equals((obj as EnumViewModel).EnumValue);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return EnumValue.GetHashCode();
        }

        public override string ToString()
        {
            return Label;
        }

        /// <summary>
        /// 获取某个枚举类型下所有可用的视图值。
        /// 
        /// 注意：不加 Label 的枚举不显示。
        /// </summary>
        /// <param name="enumType">枚举类型。注意，支持传入 Nullable(Enum)。</param>
        /// <returns></returns>
        public static List<EnumViewModel> GetByEnumType(Type enumType)
        {
            var result = new List<EnumViewModel>();

            enumType = TypeHelper.IgnoreNullable(enumType);

            foreach (Enum item in Enum.GetValues(enumType))
            {
                var label = EnumToLabel(item);
                if (!string.IsNullOrEmpty(label))
                {
                    result.Add(new EnumViewModel(item));
                }
            }

            return result;
        }

        /// <summary>
        /// 返回一个枚举值对应的 Label。
        /// 如果该枚举值没有标记 Label，则返回 null。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EnumToLabel(Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            if (null != fieldInfo)
            {
                var attri = fieldInfo.GetSingleAttribute<DisplayNameAttribute>();
                if (attri != null)
                {
                    return attri.DisplayName;
                }
            }

            return null;
        }

        /// <summary>
        /// 把 Label 或 Name 或 数值（字符串格式）解析为目标枚举类型中对应的枚举值。
        /// 注意，如果无法转换时，会抛出异常。
        /// 如果传入的是空字符串，则返回 null。
        /// </summary>
        /// <param name="labelOrNameOrValue">枚举对应的 Label、或名称、或对应的值。</param>
        /// <param name="enumType">枚举类型（不接受可空类型）</param>
        /// <param name="ignoreCase">在匹配时，是否需要忽略大小写。</param>
        /// <returns></returns>
        public static object Parse(string labelOrNameOrValue, Type enumType, bool ignoreCase = true)
        {
            //需要兼容可空的枚举，即传入 Null、String.Empty 时，返回 null。
            if (string.IsNullOrEmpty(labelOrNameOrValue)) return null;

            //先尝试解析 DisplayName。
            FieldInfo[] fields = enumType.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var attr = field.GetSingleAttribute<DisplayNameAttribute>();
                if (attr != null && string.Compare(attr.DisplayName, labelOrNameOrValue, ignoreCase) == 0)
                {
                    return field.GetValue(null);
                }
            }

            //如果 DisplayName 解析失败， 则使用名称和值的匹配。
            return Enum.Parse(enumType, labelOrNameOrValue, ignoreCase);
        }
    }
}