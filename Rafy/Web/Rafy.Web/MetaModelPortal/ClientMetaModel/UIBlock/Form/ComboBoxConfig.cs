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
using Rafy.Web.Json;

namespace Rafy.Web.ClientMetaModel
{
    /// <summary>
    /// 下拉框编辑器的配置
    /// </summary>
    public abstract class ComboBoxConfig : FieldConfig
    {
        public ComboBoxConfig()
        {
            this.xtype = "combo";
        }

        public string queryMode { get; set; }

        public string displayField { get; set; }

        public string valueField { get; set; }

        public StoreConfig store { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WritePropertyIf("queryMode", queryMode);
            json.WritePropertyIf("displayField", displayField);
            json.WritePropertyIf("valueField", valueField);
            json.WritePropertyIf("store", store);

            base.ToJson(json);
        }
    }
}