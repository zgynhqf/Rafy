/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace Rafy.Domain.DataPortal.WCF
{
    /// <summary>
    /// Response message for returning
    /// the results of a data portal call.
    /// </summary>
    [DataContract, Serializable]
    public class WcfResponse
    {
        [DataMember]
        public object Result { get; set; }

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
    }
}