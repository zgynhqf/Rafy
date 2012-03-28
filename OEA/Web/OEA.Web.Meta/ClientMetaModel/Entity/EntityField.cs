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

namespace OEA.Web.ClientMetaModel
{
    /// <summary>
    /// 实体字段定义
    /// </summary>
    public class EntityField : JsonModel
    {
        public EntityField()
        {
            this.persist = true;
        }

        public string name { get; set; }

        public ServerType type { get; set; }

        public bool persist { get; set; }

        public object defaultValue { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("name", name);
            if (!persist) json.WriteProperty("persist", persist);
            json.WritePropertyIf("defaultValue", defaultValue);
            json.WriteProperty("type", ServerTypeHelper.GetClientFieldTypeName(type), true);
        }
    }
}