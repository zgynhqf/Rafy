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
using Rafy;
using Rafy.Web.Json;

namespace Rafy.Web.EntityDataPortal
{
    /// <summary>
    /// 客户端 json 实体列表
    /// </summary>
    internal class EntityJsonList : JsonModel
    {
        internal EntityJsonList()
        {
            this.entities = new List<EntityJson>();
        }

        /// <summary>
        /// 实体列表的实际总量。
        /// 
        /// 分页时，实际量和当前量是不一致的。
        /// </summary>
        internal long total { get; set; }

        /// <summary>
        /// 客户端实体类型
        /// </summary>
        internal Type model { get; set; }

        internal List<EntityJson> entities { get; private set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("model", ClientEntities.GetClientName(model));
            json.WriteProperty("total", total);
            json.WriteProperty("entities", entities);
        }
    }

    /// <summary>
    /// 树型根对象的 json 实体
    /// </summary>
    internal class RootTreeEntityJson : TreeEntityJson
    {
        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("text", ".");

            base.ToJson(json);
        }
    }
}