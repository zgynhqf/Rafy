using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rafy.Serialization.Mobile;
using Rafy.Reflection;

namespace Rafy.Serialization
{
    /// <summary>
    /// 为了兼容，主要使用 MobileFormatter，否则使用 BinaryFormatter。
    /// </summary>
    public class MobileAndBinaryFormatter : ISerializationFormatter
    {
        public object Deserialize(Stream serializationStream)
        {
            try
            {
                var result = new MobileFormatter().Deserialize(serializationStream);

                var sysValue = result as SysState;
                if (sysValue != null)
                {
                    var sysType = Type.GetType(sysValue.TypeName);
                    result = TypeHelper.CoerceValue(sysType, sysValue.Value);
                }

                return result;
            }
            catch //(SerializationException)
            {
                //序列化失败，说明不能反序列化为 IMobile，接着尝试使用 Binary
            }

            var result2 = new BinaryFormatterWrapper().Deserialize(serializationStream);

            return result2;
        }

        public void Serialize(Stream serializationStream, object value)
        {
            if (SerializationInfoContainer.IsState(value.GetType()))
            {
                value = new SysState()
                {
                    TypeName = value.GetType().AssemblyQualifiedName,
                    Value = value
                };
            }
            else if (!(value is IMobileObject))
            {
                new BinaryFormatterWrapper().Serialize(serializationStream, value);
                return;
            }

            new MobileFormatter().Serialize(serializationStream, value);
        }
    }
}
