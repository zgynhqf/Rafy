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

namespace Rafy.MetaModel.XmlConfig.Web
{
    /// <summary>
    /// UI 块的配置
    /// </summary>
    public class BlockConfig : MetaConfig
    {
        public BlockConfig()
        {
            this.EntityProperties = new List<BlockPropertyConfig>();
            this.Commands = new List<BlockCommandConfig>();
        }

        /// <summary>
        /// 主键
        /// </summary>
        public BlockConfigKey Key { get; set; }

        /// <summary>
        /// 变更后的页面大小
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// 分组属性
        /// </summary>
        public string GroupBy { get; set; }

        /// <summary>
        /// 属性变更集
        /// </summary>
        public List<BlockPropertyConfig> EntityProperties { get; private set; }

        /// <summary>
        /// 命令变更集
        /// </summary>
        public List<BlockCommandConfig> Commands { get; private set; }

        public void Config(EntityViewMeta evm)
        {
            if (PageSize != null && evm is WebEntityViewMeta) evm.AsWebView().PageSize = PageSize.Value;
            if (GroupBy != null)
            {
                if (GroupBy == NullString)
                {
                    evm.GroupBy = null;
                }
                else
                {
                    evm.GroupBy = evm.Property(GroupBy);
                }
            }

            foreach (var property in this.EntityProperties)
            {
                if (property.IsChanged())
                {
                    var pvm = evm.Property(property.Name);
                    if (pvm != null) { property.Config(pvm); }
                }
            }

            if (RafyEnvironment.Location.IsWebUI)
            {
                foreach (var cmd in this.Commands)
                {
                    if (cmd.IsChanged())
                    {
                        var jsCmd = evm.AsWebView().Commands.Find(cmd.Name);
                        if (jsCmd != null) { cmd.Config(jsCmd); }
                    }
                }
            }
            else
            {
                foreach (var cmd in this.Commands)
                {
                    if (cmd.IsChanged())
                    {
                        var wpfCmd = evm.AsWPFView().Commands.Find(cmd.Name);
                        if (wpfCmd != null) { cmd.Config(wpfCmd); }
                    }
                }
            }
        }

        public override bool IsChanged()
        {
            var c = PageSize != null || GroupBy != null;

            c = c || this.EntityProperties.Any(p => p.IsChanged());
            c = c || this.Commands.Any(p => p.IsChanged());

            return c;
        }

        protected override XElement ToXml()
        {
            var e = this.CreateElement();
            SetAttribute(e, "PageSize", PageSize);
            SetAttribute(e, "GroupBy", GroupBy);

            //EntityProperties
            var properties = this.EntityProperties.Where(p => p.IsChanged()).ToArray();
            if (properties.Length > 0)
            {
                var pContainer = new XElement("EntityProperties");

                foreach (var property in properties)
                {
                    pContainer.Add(property.Xml);
                }

                e.Add(pContainer);
            }

            //Commands
            var commands = this.Commands.Where(p => p.IsChanged()).ToArray();
            if (commands.Length > 0)
            {
                var cmdContainer = new XElement("Commands");

                foreach (var cmd in commands)
                {
                    cmdContainer.Add(cmd.Xml);
                }

                e.Add(cmdContainer);
            }

            return e;
        }

        protected override void ReadXml(XElement element)
        {
            ReadAttribute<int>(element, "PageSize", v => PageSize = v);
            ReadAttribute<string>(element, "GroupBy", v => GroupBy = v);

            //EntityProperties
            var pContainer = element.Element("EntityProperties");
            if (pContainer != null)
            {
                foreach (var property in pContainer.Elements())
                {
                    this.EntityProperties.Add(new BlockPropertyConfig()
                    {
                        Xml = property
                    });
                }
            }

            //EntityProperties
            var cmdContainer = element.Element("Commands");
            if (cmdContainer != null)
            {
                foreach (var cmd in cmdContainer.Elements())
                {
                    this.Commands.Add(new BlockCommandConfig()
                    {
                        Xml = cmd
                    });
                }
            }
        }
    }
}