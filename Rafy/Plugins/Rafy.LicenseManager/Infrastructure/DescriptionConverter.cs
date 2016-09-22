/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20160921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20160921 10:00
 * 
*******************************************************/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Rafy.LicenseManager.Entities;

namespace Rafy.LicenseManager.Infrastructure
{
    internal sealed class DescriptionConverter : EnumConverter
    {
        private static readonly MemberInfo[] _members;

        static DescriptionConverter()
        {
            _members = typeof(LicenseTarget).GetMembers(BindingFlags.Public | BindingFlags.Static);
        }

        public DescriptionConverter(Type type) : base(type)
        {

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(string) || value == null)
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            LicenseTarget target;
            if (!Enum.TryParse(value.ToString(), out target))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            var member = _members.FirstOrDefault(m => m.Name == value.ToString());
            if (member == null)
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            var attribute = member.GetCustomAttribute(typeof(DescriptionAttribute), false);

            return attribute != null ? ((DescriptionAttribute) attribute).Description : base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }
    }
}
