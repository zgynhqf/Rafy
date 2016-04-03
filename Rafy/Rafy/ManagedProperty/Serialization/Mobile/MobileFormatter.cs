using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Serializes and deserializes objects
    /// at the field level. A Silverlight-
    /// compatible facsimile of the
    /// BinaryFormatter or NetDataContractSerializer.
    /// </summary>
#if TESTING
  [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class MobileFormatter : ISerializationFormatter
    {
        #region 实例门面接口

        public void Serialize(XmlWriter writer, object graph)
        {
            var info = SerializeCore(graph);

            var dc = GetDataContractSerializer();
            dc.WriteObject(writer, info);
        }

        public void Serialize(Stream stream, object graph)
        {
            var info = SerializeCore(graph);

            var dc = GetDataContractSerializer();
            dc.WriteObject(stream, info);
        }

        public object Deserialize(XmlReader reader)
        {
            var dc = GetDataContractSerializer();

            var info = dc.ReadObject(reader) as List<SerializationInfoContainer>;

            return DeserializeCore(info);
        }

        public object Deserialize(Stream stream)
        {
            var dc = GetDataContractSerializer();

            var info = dc.ReadObject(stream) as List<SerializationInfoContainer>;

            return DeserializeCore(info);
        }

        //void ISerializationFormatter.Serialize(Stream stream, object graph)
        //{
        //    this.Serialize(stream, graph);
        //}

        //object ISerializationFormatter.Deserialize(Stream stream)
        //{
        //    return this.Deserialize(stream);
        //}

        #endregion

        #region 静态门面接口

        public static string SerializeToString(object obj)
        {
            return Encoding.UTF8.GetString(Serialize(obj));
        }

        public static string SerializeToXml(object obj)
        {
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw))
            {
                var formatter = new MobileFormatter();
                formatter.Serialize(xw, obj);
            }
            return sw.ToString();
        }

        /// <summary>
        /// Serializes the object into a byte array.
        /// </summary>
        /// <param name="obj">
        /// The object to be serialized, which must implement
        /// IMobileObject.
        /// </param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            using (var buffer = new MemoryStream())
            {
                var formatter = new MobileFormatter();
                formatter.Serialize(buffer, obj);
                return buffer.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a byte stream into an object.
        /// </summary>
        /// <param name="data">
        /// Byte array containing the object's serialized
        /// data.
        /// </param>
        /// <returns>
        /// An object containing the data from the
        /// byte stream. The object must implement
        /// IMobileObject to be deserialized.
        /// </returns>
        public static object Deserialize(byte[] data)
        {
            using (var buffer = new MemoryStream(data))
            {
                var formatter = new MobileFormatter();
                return formatter.Deserialize(buffer);
            }
        }

        #endregion

        #region SerializeCore

        private Dictionary<IMobileObject, SerializationInfoContainer> _serializationReferences =
            new Dictionary<IMobileObject, SerializationInfoContainer>(new ReferenceComparer<IMobileObject>());

        private List<SerializationInfoContainer> SerializeCore(object graph)
        {
            this._serializationReferences.Clear();

            SerializeObject(graph);

            var serialized = this._serializationReferences.Values.ToList();

            return serialized;
        }

        /// <summary>
        /// Serializes an object into a SerializationInfo object.
        /// </summary>
        /// <param name="obj">Object to be serialized.</param>
        /// <returns></returns>
        public SerializationInfoContainer SerializeObject(object obj)
        {
            SerializationInfoContainer info;
            if (obj == null)
            {
                if (!this._serializationReferences.TryGetValue(NullPlaceholder.Instance, out info))
                {
                    info = new SerializationInfoContainer(this._serializationReferences.Count + 1)
                    {
                        TypeName = NullPlaceholder.TypeShortName
                    };

                    this._serializationReferences.Add(NullPlaceholder.Instance, info);
                }
            }
            else
            {
                var thisType = obj.GetType();
                //if (thisType.IsSerializable)
                //{
                //    throw new InvalidOperationException(string.Format("Object Not Serializable Formatted: {0}", thisType.FullName));
                //}

                var mobile = obj as IMobileObject;
                if (mobile == null)
                {
                    throw new InvalidOperationException(string.Format("{0} must implement IMobileObject", thisType.Name));
                }

                if (!this._serializationReferences.TryGetValue(mobile, out info))
                {
                    info = new SerializationInfoContainer(this._serializationReferences.Count + 1);
                    this._serializationReferences.Add(mobile, info);

                    info.TypeName = thisType.AssemblyQualifiedName;

                    var context = new SerializationContainerContext(info, this);
                    mobile.SerializeRef(context);
                    context.IsProcessingState = true;
                    mobile.SerializeState(context);
                }
            }

            return info;
        }

        public int SerialzeRef(object obj)
        {
            var si = this.SerializeObject(obj);
            return si.ReferenceId;
        }

        #endregion

        #region DeserializeCore

        private Dictionary<int, IMobileObject> _deserializationReferences = new Dictionary<int, IMobileObject>();

        private object DeserializeCore(List<SerializationInfoContainer> deserialized)
        {
            this._deserializationReferences = new Dictionary<int, IMobileObject>();

            var context = new SerializationContainerContext(null, this);
            context.IsProcessingState = true;

            foreach (SerializationInfoContainer info in deserialized)
            {
                if (info.TypeName == NullPlaceholder.TypeShortName)
                {
                    this._deserializationReferences.Add(info.ReferenceId, null);
                }
                else
                {
                    Type type = Type.GetType(info.TypeName);

                    if (type == null)
                    {
                        throw new SerializationException(string.Format("MobileFormatter unable to deserialize {0}", info.TypeName));
                    }

                    IMobileObject mobile = null;

                    try
                    {
                        mobile = (IMobileObject)Activator.CreateInstance(type, true);
                    }
                    catch (MissingMethodException)
                    {
                        throw new InvalidOperationException(string.Format("类型 {0} 必须拥有无参数的构造函数才能支持序列化！", type.FullName));
                    }

                    this._deserializationReferences.Add(info.ReferenceId, mobile);

                    context.Container = info;
                    mobile.DeserializeState(context);
                }
            }

            context.IsProcessingState = false;
            foreach (SerializationInfoContainer info in deserialized)
            {
                IMobileObject mobile = this._deserializationReferences[info.ReferenceId];

                if (mobile != null)
                {
                    context.Container = info;
                    mobile.DeserializeRef(context);
                }
            }

            foreach (SerializationInfoContainer info in deserialized)
            {
                var notifiable = this._deserializationReferences[info.ReferenceId] as ISerializationNotification;
                if (notifiable != null)
                {
                    context.Container = info;
                    notifiable.Deserialized(context);
                }
            }

            return this._deserializationReferences.Count > 0 ? this._deserializationReferences[1] : null;
        }

        /// <summary>
        /// Gets a deserialized object based on the object's
        /// reference id within the serialization stream.
        /// </summary>
        /// <param name="referenceId">Id of object in stream.</param>
        /// <returns></returns>
        public IMobileObject GetObject(int referenceId)
        {
            return this._deserializationReferences[referenceId];
        }

        public T GetObject<T>(int referenceId)
        {
            return (T)this._deserializationReferences[referenceId];
        }

        #endregion

        private static XmlObjectSerializer GetDataContractSerializer()
        {
            var knownTypes = new Type[] { typeof(List<int>), typeof(byte[]), typeof(DateTimeOffset), typeof(DateTime) };

            //return new DataContractSerializer(typeof(List<SerializationContainer>), knownTypes);

            //return new NetDataContractSerializer();

            return new DataContractJsonSerializer(typeof(List<SerializationInfoContainer>), knownTypes);
        }
    }
}