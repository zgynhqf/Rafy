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
    /// JsCommand 的配置器
    /// </summary>
    public class BlockCommandConfig : MetaConfig
    {
        public string Name { get; set; }

        public string Label { get; set; }

        public bool? IsVisible { get; set; }

        internal void Config(WebCommand cmd)
        {
            if (Label != null)
            {
                //这句会同时设置 LabelModified 属性的值。
                cmd.HasLabel(Label);
            }

            if (IsVisible != null) { cmd.IsVisible = IsVisible.Value; }
        }

        internal void Config(WPFCommand cmd)
        {
            if (Label != null)
            {
                cmd.HasLabel(Label);
            }

            if (IsVisible != null) { cmd.IsVisible = IsVisible.Value; }
        }

        public override bool IsChanged()
        {
            return Label != null || IsVisible != null;
        }

        protected override string GetXName()
        {
            return "Command";
        }

        protected override XElement ToXml()
        {
            var e = this.CreateElement();

            e.Add(new XAttribute("Name", Name));
            if (Label != null) { e.Add(new XAttribute("Label", Label)); }
            if (IsVisible != null) { e.Add(new XAttribute("IsVisible", IsVisible.Value)); }

            return e;
        }

        protected override void ReadXml(XElement element)
        {
            var a = element.Attribute("Label");
            if (a != null) { Label = TypeHelper.CoerceValue<string>(a.Value); }

            a = element.Attribute("IsVisible");
            if (a != null) { IsVisible = TypeHelper.CoerceValue<bool>(a.Value); }

            this.Name = element.Attribute("Name").Value;
        }
    }
}