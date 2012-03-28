/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Web.Json;

namespace OEA.Web.EntityDataPortal
{
    /// <summary>
    /// 实体数据类
    /// </summary>
    internal class EntityJson : DynamicJsonModel { }

    /// <summary>
    /// 树型实体数据类
    /// </summary>
    internal class TreeEntityJson : EntityJson
    {
        internal TreeEntityJson()
        {
            this.children = new List<TreeEntityJson>();
        }

        internal bool expanded { get; set; }

        internal List<TreeEntityJson> children { get; private set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            if (children.Count > 0) { json.WriteProperty("entities", children); }
            else { json.WriteProperty("leaf", true); }

            //if (expanded) json.WriteProperty("expanded", expanded);

            if (!this.HasDynamicProperty) { this.SetProperty("e", "1"); }

            base.ToJson(json);
        }
    }
}