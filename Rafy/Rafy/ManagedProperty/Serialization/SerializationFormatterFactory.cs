using System;
using Rafy.Serialization.Mobile;

namespace Rafy.Serialization
{
    /// <summary>
    /// Factory used to create the appropriate
    /// serialization formatter object based
    /// on the application configuration.
    /// </summary>
    public static class SerializationFormatterFactory
    {
        /// <summary>
        /// 创建一个序列化器。
        /// 
        /// 工厂方法，方便未来统一扩展。
        /// </summary>
        public static ISerializationFormatter GetFormatter()
        {
            //return new MobileAndBinaryFormatter();

            return new BinaryFormatterWrapper();

            //return new NetDataContractSerializerWrapper();
        }
    }
}
