using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using OEA.MetaModel.Attributes;

namespace OEA.Utils
{
    /// <summary>
    /// 枚举值的逻辑视图
    /// </summary>
    public class EnumViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">枚举值</param>
        public EnumViewModel(Enum value)
        {
            this.EnumValue = value;

            Type type = value.GetType();
            FieldInfo fieldInfo = type.GetField(value.ToString());
            if (null != fieldInfo)
            {
                var attri = fieldInfo.GetSingleAttribute<LabelAttribute>();
                if (attri != null)
                {
                    this.Label = attri.Label;
                }
                else
                {
                    this.Label = value.ToString();
                }
            }
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
        /// EnumTreeColumn 和其它地方中使用了这个方法来对比实体。
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

        public static List<EnumViewModel> GetByEnumType(Type enumType)
        {
            List<EnumViewModel> result = new List<EnumViewModel>();

            foreach (Enum item in Enum.GetValues(enumType)) { result.Add(new EnumViewModel(item)); }

            return result;
        }

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

        public static string EnumToLabel(Enum value)
        {
            return new EnumViewModel(value).Label;
        }
    }
}