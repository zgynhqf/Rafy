using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using OEA.MetaModel.Attributes;
using OEA.Reflection;

namespace OEA.Utils
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
        }

        private EnumViewModel(Enum value, string label)
        {
            this.EnumValue = value;
            this.Label = label;
        }

        /// <summary>
        /// 枚举值
        /// </summary>
        public Enum EnumValue { get; private set; }

        /// <summary>
        /// 枚举值的显示
        /// </summary>
        public string Label { get; private set; }

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
            List<EnumViewModel> result = new List<EnumViewModel>();

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
        /// 把 Label 解析为目标枚举类型中对应的枚举值。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="enumType">枚举类型（不接受可空类型）</param>
        /// <returns></returns>
        public static object LabelToEnum(string str, Type enumType)
        {
            FieldInfo[] fieldInfos = enumType.GetFields();

            foreach (var item in fieldInfos)
            {
                var attr = item.GetSingleAttribute<LabelAttribute>();
                if (attr != null && attr.Label == str)
                {
                    return Enum.Parse(enumType, item.Name, true);
                }
            }

            //如果在 Label 中没有找到，那这个 label 其实就是枚举本身的名称。
            foreach (var item in fieldInfos)
            {
                if (item.Name == str)
                {
                    return Enum.Parse(enumType, item.Name, true);
                }
            }

            return null;
        }

        /// <summary>
        /// 返回一个枚举值对应的 Label。
        /// 如果该枚举值没有标记 Label，则返回 String.Empty。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string EnumToLabel(Enum value)
        {
            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            if (null != fieldInfo)
            {
                var attri = fieldInfo.GetSingleAttribute<LabelAttribute>();
                if (attri != null)
                {
                    return attri.Label;
                }
            }

            return null;
        }
    }
}