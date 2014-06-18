/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130406 10:20
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130406 10:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rafy.DomainModeling.Models.Xml
{
    [XmlType("EntityTypeElement")]
    public class EntityTypeElementXml
    {
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string Label { get; set; }
        [XmlAttribute]
        public double Left { get; set; }
        [XmlAttribute]
        public double Top { get; set; }
        [XmlAttribute]
        public double Width { get; set; }
        [XmlAttribute]
        public double Height { get; set; }
        [XmlAttribute]
        public bool HideProperties { get; set; }
        [XmlAttribute]
        public bool IsAggtRoot { get; set; }

        public PropertyElementXml[] Properties { get; set; }

        internal static EntityTypeElementXml ConvertToNode(EntityTypeElement model)
        {
            var xml = new EntityTypeElementXml();

            xml.FullName = model.FullName;
            xml.Label = model.Label;
            xml.Left = model.Left;
            xml.Top = model.Top;
            xml.Width = model.Width;
            xml.Height = model.Height;
            xml.HideProperties = model.HideProperties;
            xml.IsAggtRoot = model.IsAggtRoot;

            xml.Properties = new PropertyElementXml[model.Properties.Count];
            for (int i = 0, c = model.Properties.Count; i < c; i++)
            {
                var property = model.Properties[i];
                var propertyXml = PropertyElementXml.ConvertToNode(property);
                xml.Properties[i] = propertyXml;
            }

            return xml;
        }

        internal EntityTypeElement Restore()
        {
            var model = new EntityTypeElement(this.FullName);

            model.Label = this.Label;
            model.Left = this.Left;
            model.Top = this.Top;
            model.Width = this.Width;
            model.Height = this.Height;
            model.HideProperties = this.HideProperties;
            model.IsAggtRoot = this.IsAggtRoot;

            foreach (var item in this.Properties)
            {
                var modelItem = item.Restore();
                model.Properties.Add(modelItem);
            }

            return model;
        }
    }
}
