using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using Rafy;

namespace Rafy
{
    public static class SerializationHelper
    {
        public static string XmlSerialize(object graph)
        {
            var xmlFormatter = new XmlFormatter(graph.GetType());
            var xml = xmlFormatter.Serialize(graph);
            return xml;
        }

        public static object XmlDeserialize(Type type, string xml)
        {
            var xmlFormatter = new XmlFormatter(type);
            var graph = xmlFormatter.Deserialize(xml);
            return graph;
        }

        public static T XmlDeserialize<T>(string xml)
        {
            return (T)XmlDeserialize(typeof(T), xml);
        }

        //internal static string XmlSerialize(object graph)
        //{
        //    using (var buffer = new MemoryStream())
        //    {
        //        XmlWriter writer = XmlWriter.Create(buffer);
        //        DataContractSerializer dcs = new DataContractSerializer(graph.GetType());
        //        dcs.WriteObject(writer, graph);
        //        writer.Flush();
        //        byte[] data = buffer.ToArray();
        //        return Encoding.UTF8.GetString(data, 0, data.Length);
        //    }
        //}

        //internal static T XmlDeserialize<T>(string xml)
        //{
        //    using (var buffer = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        //    {
        //        XmlReader reader = XmlReader.Create(buffer);
        //        DataContractSerializer dcs = new DataContractSerializer(typeof(T));
        //        return (T)dcs.ReadObject(reader);
        //    }
        //}
    }
}