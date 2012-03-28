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
    /// 客户端的某一小块的元数据定义
    /// </summary>
    public class ClientEntityMeta : JsonModel
    {
        /// <summary>
        /// 对应的客户端类的名称
        /// </summary>
        public string model { get; set; }

        /// <summary>
        /// 如果当前块不是默认视图，则这个属性表示它的扩展视图名称
        /// </summary>
        public string viewName { get; set; }

        /// <summary>
        /// 当前模型的友好名称
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// 如果是列表，则按哪个属性分组。
        /// </summary>
        public string groupBy { get; set; }

        /// <summary>
        /// 是否树型对象
        /// </summary>
        public bool isTree { get; set; }

        /// <summary>
        /// 如果此块是列表，则这是客户端 ExtGrid/ExtTreeGrid 的配置项
        /// </summary>
        public GridConfig gridConfig { get; set; }

        /// <summary>
        /// 如果此块是表单，则这是客户端 ExtForm 的配置项
        /// </summary>
        public FormConfig formConfig { get; set; }

        /// <summary>
        /// 列表块中所用的 Store 配置
        /// </summary>
        public AbstractStoreConfig storeConfig { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WritePropertyIf("model", model);
            json.WritePropertyIf("viewName", viewName);
            json.WritePropertyIf("label", label);
            json.WritePropertyIf("groupBy", groupBy);
            json.WritePropertyIf("gridConfig", gridConfig);
            json.WritePropertyIf("formConfig", formConfig);
            json.WritePropertyIf("storeConfig", storeConfig);
            json.WriteProperty("isTree", isTree, true);
        }
    }
}