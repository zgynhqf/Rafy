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
    [XmlType("PropertyElement")]
    public class PropertyElementXml
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string PropertyType { get; set; }
        [XmlAttribute]
        public string Label { get; set; }

        internal static PropertyElementXml ConvertToNode(PropertyElement model)
        {
            var xml = new PropertyElementXml();

            xml.Name = model.Name;
            xml.PropertyType = model.PropertyType;
            xml.Label = model.Label;

            return xml;
        }

        internal PropertyElement Restore()
        {
            var model = new PropertyElement(this.Name);

            model.Label = this.Label;
            model.PropertyType = this.PropertyType;

            return model;
        }
    }
}
