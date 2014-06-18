/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 22:28
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 22:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rafy.DomainModeling.Models.Xml
{
    /// <summary>
    /// Document 的 xml 序列化器。
    /// </summary>
    [XmlType("Document")]
    public class DocumentXml
    {
        [XmlAttribute]
        public bool HideNonsenceLabels { get; set; }

        public EntityTypeElementXml[] EntityTypes { get; set; }

        public EnumElementXml[] EnumTypes { get; set; }

        public ConnectionElementXml[] Connections { get; set; }

        internal string ToXml()
        {
            return Rafy.SerializationHelper.XmlSerialize(this);
        }

        internal static DocumentXml FromXml(string xml)
        {
            return Rafy.SerializationHelper.XmlDeserialize(typeof(DocumentXml), xml) as DocumentXml;
        }

        internal static DocumentXml ConvertToXmlDoc(ODMLDocument document)
        {
            var xml = new DocumentXml();
            xml.HideNonsenceLabels = document.HideNonsenceLabels;

            xml.EntityTypes = new EntityTypeElementXml[document.EntityTypes.Count];
            for (int i = 0, c = document.EntityTypes.Count; i < c; i++)
            {
                var item = document.EntityTypes[i];
                xml.EntityTypes[i] = EntityTypeElementXml.ConvertToNode(item);
            }

            xml.EnumTypes = new EnumElementXml[document.EnumTypes.Count];
            for (int i = 0, c = document.EnumTypes.Count; i < c; i++)
            {
                var item = document.EnumTypes[i];
                xml.EnumTypes[i] = EnumElementXml.ConvertToNode(item);
            }

            xml.Connections = new ConnectionElementXml[document.Connections.Count];
            for (int i = 0, c = document.Connections.Count; i < c; i++)
            {
                var item = document.Connections[i];
                xml.Connections[i] = ConnectionElementXml.ConvertToNode(item);
            }

            return xml;
        }

        internal ODMLDocument Restore()
        {
            var doc = new ODMLDocument();
            doc.HideNonsenceLabels = this.HideNonsenceLabels;

            foreach (var item in this.EntityTypes)
            {
                var con = item.Restore();
                doc.EntityTypes.Add(con);
            }

            foreach (var item in this.EnumTypes)
            {
                var con = item.Restore();
                doc.EnumTypes.Add(con);
            }

            foreach (var item in this.Connections)
            {
                var con = item.Restore();
                doc.Connections.Add(con);
            }

            return doc;
        }
    }
}