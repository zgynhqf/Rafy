using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OEA
{
    public static class SerializationInfoExtension
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            var value = info.GetValue(name, typeof(T));
            if (value == null) return default(T);
            return (T)value;
        }
    }
}
