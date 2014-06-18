/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130406 10:22
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130406 10:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace Rafy.DomainModeling.Models.Xml
{
    [XmlType("ConnectionElement")]
    public class ConnectionElementXml
    {
        [XmlAttribute]
        public string From { get; set; }
        [XmlAttribute]
        public string To { get; set; }
        [XmlAttribute]
        public string Label { get; set; }
        [XmlAttribute]
        public bool Hidden { get; set; }
        [XmlAttribute]
        public bool LabelVisible { get; set; }
        [XmlAttribute]
        public ConnectionType ConnectionType { get; set; }
        public double? FromPointX { get; set; }
        public double? FromPointY { get; set; }
        public double? ToPointX { get; set; }
        public double? ToPointY { get; set; }

        internal static ConnectionElementXml ConvertToNode(ConnectionElement model)
        {
            var xml = new ConnectionElementXml();

            xml.From = model.From;
            xml.To = model.To;
            xml.Label = model.Label;
            xml.ConnectionType = model.ConnectionType;
            xml.Hidden = model.Hidden;
            xml.LabelVisible = model.LabelVisible;

            if (model.FromPointPos.HasValue)
            {
                xml.FromPointX = model.FromPointPos.Value.X;
                xml.FromPointY = model.FromPointPos.Value.Y;
            }
            if (model.ToPointPos.HasValue)
            {
                xml.ToPointX = model.ToPointPos.Value.X;
                xml.ToPointY = model.ToPointPos.Value.Y;
            }

            return xml;
        }

        internal ConnectionElement Restore()
        {
            var model = new ConnectionElement(this.From, this.To);

            model.Label = this.Label;
            model.ConnectionType = this.ConnectionType;
            model.Hidden = this.Hidden;
            model.LabelVisible = this.LabelVisible;

            if (this.FromPointX.HasValue && this.FromPointY.HasValue)
            {
                model.FromPointPos = new Point(this.FromPointX.Value, this.FromPointY.Value);
            }
            if (this.ToPointX.HasValue && this.ToPointY.HasValue)
            {
                model.ToPointPos = new Point(this.ToPointX.Value, this.ToPointY.Value);
            }

            return model;
        }
    }
}