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
    [XmlType("EnumItemElement")]
    public class EnumItemElementXml
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Label { get; set; }
        //[XmlAttribute]
        //public int Value { get; set; }

        internal static EnumItemElementXml ConvertToNode(EnumItemElement model)
        {
            var xml = new EnumItemElementXml();

            xml.Name = model.Name;
            xml.Label = model.Label;
            //xml.Value = model.Value;

            return xml;
        }

        internal EnumItemElement Restore()
        {
            var model = new EnumItemElement(this.Name);

            model.Label = this.Label;
            //model.Value = this.Value;

            return model;
        }
    }
}
