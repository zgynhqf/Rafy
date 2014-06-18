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
    /// 字段编辑器的配置
    /// </summary>
    public class FieldConfig : DynamicJsonModel
    {
        public string name { get; set; }
        public string fieldLabel { get; set; }
        public string anchor { get; set; }
        public string xtype { get; set; }
        public bool isReadonly { get; set; }

        /// <summary>
        /// 这个属性不为空，表明是否需要动态根据某个属性来设置本编辑器的显示或者隐藏。
        /// </summary>
        public string visibilityIndicator { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WritePropertyIf("name", name);
            json.WritePropertyIf("anchor", anchor);
            json.WritePropertyIf("fieldLabel", fieldLabel);
            json.WritePropertyIf("visibilityIndicator", visibilityIndicator);
            if (isReadonly) { json.WriteProperty("readOnly", isReadonly); }

            this.SetProperty("xtype", xtype);
            base.ToJson(json);
        }
    }
}
