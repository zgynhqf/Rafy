/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130406 00:20
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130406 00:20
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Rafy.DomainModeling.Models.Xml
{
    [XmlType("EnumElement")]
    public class EnumElementXml
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

        public EnumItemElementXml[] Items { get; set; }

        internal static EnumElementXml ConvertToNode(EnumElement model)
        {
            var xml = new EnumElementXml();

            xml.FullName = model.Name;
            xml.Label = model.Label;
            xml.Left = model.Left;
            xml.Top = model.Top;
            xml.Width = model.Width;
            xml.Height = model.Height;

            xml.Items = new EnumItemElementXml[model.Items.Count];
            for (int i = 0, c = model.Items.Count; i < c; i++)
            {
                var property = model.Items[i];
                var propertyXml = EnumItemElementXml.ConvertToNode(property);
                xml.Items[i] = propertyXml;
            }

            return xml;
        }

        internal EnumElement Restore()
        {
            var model = new EnumElement(this.FullName);

            model.Label = this.Label;
            model.Left = this.Left;
            model.Top = this.Top;
            model.Width = this.Width;
            model.Height = this.Height;

            foreach (var item in this.Items)
            {
                var modelItem = item.Restore();
                model.Items.Add(modelItem);
            }

            return model;
        }
    }
}