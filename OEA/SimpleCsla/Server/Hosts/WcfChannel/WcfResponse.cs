using System;
using System.Runtime.Serialization;
using SimpleCsla.Serialization.Mobile;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleCsla.Server.Hosts.WcfChannel
{
    /// <summary>
    /// Response message for returning
    /// the results of a data portal call.
    /// </summary>
    [DataContract]
    public class WcfResponse
    {
        [DataMember]
        private object _result;

        /// <summary>
        /// Create new instance of object.
        /// </summary>
        /// <param name="result">Result object to be returned.</param>
        public WcfResponse(object result)
        {
            this.Result = result;
        }

        ///// <summary>
        ///// Criteria object describing business object.
        ///// </summary>
        //public object Result
        //{
        //    get
        //    {
        //        return this._result;
        //    }
        //    set
        //    {
        //        this._result = value;
        //    }
        //}

        //[DataMember]
        //private bool _isSerialized;

        //public object Result
        //{
        //    get
        //    {
        //        if (!this._isSerialized) return this._result;

        //        var stream = new MemoryStream();
        //        using (var sw = new StreamWriter(stream))
        //        {
        //            sw.Write(this._result.ToString());
        //        }
        //        stream.Position = 0;
        //        var reader = XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max);

        //        var formatter = new MobileFormatter();
        //        return formatter.Deserialize(reader);
        //    }
        //    set
        //    {
        //        var mobileObject = value as IMobileObject;
        //        if (mobileObject != null)
        //        {
        //            var stream = new MemoryStream();
        //            var writer = XmlDictionaryWriter.CreateTextWriter(stream);

        //            var formatter = new MobileFormatter();
        //            formatter.Serialize(writer, mobileObject);
        //            writer.Flush();

        //            stream.Position = 0;
        //            using (var reader = new StreamReader(stream))
        //            {
        //                this._result = reader.ReadToEnd();
        //                this._isSerialized = true;
        //            }
        //        }
        //        else
        //        {
        //            this._result = value;
        //            this._isSerialized = false;
        //        }
        //    }
        //}

        //public object Result
        //{
        //    get
        //    {
        //        var stream = new MemoryStream();
        //        using (var sw = new StreamWriter(stream))
        //        {
        //            sw.Write(this._result.ToString());
        //        }

        //        stream.Position = 0;
        //        var reader = XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max);

        //        var serializer = GetSerializer();
        //        var value = serializer.ReadObject(reader) as DTO;
        //        return value.Value;
        //    }
        //    set
        //    {
        //        var stream = new MemoryStream();
        //        var writer = XmlDictionaryWriter.CreateTextWriter(stream);

        //        var serializer = GetSerializer();
        //        serializer.WriteObject(writer, new DTO() { Value = value });
        //        writer.Flush();

        //        stream.Position = 0;
        //        using (var reader = new StreamReader(stream))
        //        {
        //            this._result = reader.ReadToEnd();
        //        }
        //    }
        //}

        //private XmlObjectSerializer GetSerializer()
        //{
        //    return new DataContractJsonSerializer(typeof(List<DTO>));
        //}

        //[DataContract]
        //private class DTO
        //{
        //    [DataMember]
        //    public object Value;
        //}

        public object Result
        {
            get
            {
                return InnerSerializer.DeserializeObject(this._result);
            }
            set
            {
                this._result = InnerSerializer.SerializeObject(value);
            }
        }
    }
}