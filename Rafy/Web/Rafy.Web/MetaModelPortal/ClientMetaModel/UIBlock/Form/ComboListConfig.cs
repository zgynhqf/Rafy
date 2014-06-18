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
    /// 下拉列表编辑器的配置
    /// </summary>
    public class ComboListConfig : ComboBoxConfig
    {
        public ComboListConfig()
        {
            this.xtype = "combolist";
        }

        /// <summary>
        /// 下拉列表中显示的实体名称。
        /// </summary>
        public string model { get; set; }

        /// <summary>
        /// 如果是自定义数据源，则这个属性表示自定义数据源的属性名称。
        /// </summary>
        public string dataSourceProperty { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            //当使用自定义数据源时，强制其使用本地数据。
            if (!string.IsNullOrEmpty(this.dataSourceProperty))
            {
                this.queryMode = "local";
            }

            json.WritePropertyIf("model", model);
            json.WritePropertyIf("dataSourceProperty", dataSourceProperty);

            base.ToJson(json);
        }
    }
}