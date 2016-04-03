/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120226
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120226
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.View;
using System.Xml.Linq;
using Rafy.Reflection;

namespace Rafy.MetaModel.XmlConfig.Web
{
    /// <summary>
    /// UI 块界面属性的配置
    /// </summary>
    public class BlockPropertyConfig : MetaConfig
    {
        /// <summary>
        /// 属性的名称
        /// 此属性不可为空
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 变更后的标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 变更后的显示信息
        /// </summary>
        public ShowInWhere? ShowInWhere { get; set; }

        /// <summary>
        /// 变更后的位置
        /// </summary>
        public double? OrderNo { get; set; }

        internal void Config(EntityPropertyViewMeta property)
        {
            if (this.Label != null) { property.Label = this.Label; }

            if (this.ShowInWhere != null) { property.ShowInWhere = this.ShowInWhere.Value; }

            if (this.OrderNo != null) { property.OrderNo = this.OrderNo.Value; }
        }

        public override bool IsChanged()
        {
            return this.ShowInWhere != null || this.OrderNo != null || this.Label != null;
        }

        protected override string GetXName()
        {
            return "EntityProperty";
        }

        protected override XElement ToXml()
        {
            var e = this.CreateElement();

            e.Add(new XAttribute("Name", Name));
            if (ShowInWhere != null) { e.Add(new XAttribute("ShowInWhere", ShowInWhere.Value)); }
            if (OrderNo != null) { e.Add(new XAttribute("OrderNo", OrderNo.Value)); }
            if (Label != null) { e.Add(new XAttribute("Label", Label)); }

            return e;
        }

        protected override void ReadXml(XElement element)
        {
            var a = element.Attribute("ShowInWhere");
            if (a != null) { ShowInWhere = TypeHelper.CoerceValue<ShowInWhere>(a.Value); }

            a = element.Attribute("OrderNo");
            if (a != null) { OrderNo = TypeHelper.CoerceValue<double>(a.Value); }

            a = element.Attribute("Label");
            if (a != null) { Label = TypeHelper.CoerceValue<string>(a.Value); }

            Name = element.Attribute("Name").Value;
        }
    }
}