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
    /// 数组数据源配置
    /// </summary>
    internal class ArrayStoreConfig : StoreConfig
    {
        public string[] fields { get; set; }

        public JsonModel[] data { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            //json.WriteProperty("autoDestroy", true);

            json.WritePropertyIf("fields", fields);
            json.WritePropertyIf("data", data);

            base.ToJson(json);
        }
    }
}