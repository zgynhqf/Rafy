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
            if (destinationType == typeof(string) && value != null)
            {
                LicenseTarget target;
                if (!Enum.TryParse(value.ToString(), out target))
                {
                    return base.ConvertTo(context, culture, value, destinationType);
                }

                var member = _members.FirstOrDefault(m => m.Name == value.ToString());

                if (member != null)
                {
                    var attribute = member.GetCustomAttributes(typeof(DescriptionAttribute)).FirstOrDefault();

                    if (attribute != null)
                        return ((DescriptionAttribute) attribute).Description;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
