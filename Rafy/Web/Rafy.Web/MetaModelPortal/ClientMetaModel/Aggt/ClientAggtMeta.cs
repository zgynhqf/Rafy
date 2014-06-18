/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120310
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120310
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Web.Json;

namespace Rafy.Web.ClientMetaModel
{
    /// <summary>
    /// 客户端的聚合元数据
    /// </summary>
    public class ClientAggtMeta : JsonModel
    {
        public ClientAggtMeta()
        {
            this.children = new List<ClientAggtMeta>();
            this.surrounders = new List<SurrounderMeta>();
        }

        /// <summary>
        /// 如果当前聚合块是一个子类型，则这个属性表示这个聚合子类型在聚合父类型中的集合属性名称。
        /// </summary>
        public string childProperty { get; set; }

        /// <summary>
        /// 当前聚合块使用哪种布局。
        /// </summary>
        public string layoutClass { get; set; }

        /// <summary>
        /// 当前聚合块中主块
        /// </summary>
        public ClientEntityViewMeta mainBlock { get; set; }

        /// <summary>
        /// 所有的子聚合块
        /// </summary>
        public IList<ClientAggtMeta> children { get; private set; }

        /// <summary>
        /// 所有的环绕块
        /// </summary>
        public IList<SurrounderMeta> surrounders { get; private set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            if (mainBlock == null) throw new ArgumentNullException("mainBlock");

            json.WritePropertyIf("layoutClass", layoutClass);
            json.WritePropertyIf("childProperty", childProperty);
            json.WriteProperty("mainBlock", mainBlock);
            json.WritePropertyIf("children", children);
            json.WritePropertyIf("surrounders", surrounders);
        }
    }

    /// <summary>
    /// 环绕块
    /// </summary>
    public class SurrounderMeta : ClientAggtMeta
    {
        /// <summary>
        /// 环绕类型
        /// </summary>
        public string surrounderType { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WritePropertyIf("surrounderType", surrounderType);

            base.ToJson(json);
        }
    }
}