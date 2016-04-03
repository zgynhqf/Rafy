using System;
using System.Data;
using System.Configuration;
using System.Web;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 一个简单的二进制序列化类
    /// </summary>
    public class BytesFormatter : IStateFormatter
    {
        /// <summary>
        /// Current Encoding to serialize/deserialize the object.
        /// </summary>
        public static Encoding NowEncoding
        {
            get { return Encoding.UTF8; }
        }

        private BinaryFormatter bf;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BytesFormatter()
        {
            bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        }

        #endregion

        /// <summary>
        /// Serialize the object to bytes
        /// </summary>
        /// <param name="x">the object to be serialized.(The type should marked a [Serializable] attribute)</param>
        /// <returns></returns>
        public byte[] SerializeToBytes(object x)
        {
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, x);
            return ms.GetBuffer();
        }

        /// <summary>
        /// Serialize the object to string
        /// </summary>
        /// <param name="x">the object to be serialized.(The type should marked a [Serializable] attribute)</param>
        /// <returns></returns>
        public string Serialize(object x)
        {
            byte[] bytes = this.SerializeToBytes(x);
            return NowEncoding.GetString(bytes);
        }

        /// <summary>
        /// Deserialize the bytes to original object
        /// </summary>
        /// <param name="binaryData">the data which is storing the state of a object.</param>
        /// <returns></returns>
        public object Deserialize(byte[] binaryData)
        {
            MemoryStream ms = new MemoryStream(binaryData);
            return bf.Deserialize(ms);
        }

        /// <summary>
        /// Deserialize the string to original object
        /// </summary>
        /// <param name="Data">the data which is storing the state of a object.</param>
        /// <returns></returns>
        public object Deserialize(string Data)
        {
            byte[] bytes = NowEncoding.GetBytes(Data);
            return this.Deserialize(bytes);
        }
    }
}