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
using OEA.Utils;

namespace OEA.Web.ClientMetaModel
{
    /// <summary>
    /// 枚举的编辑器配置
    /// </summary>
    internal class EnumBoxConfig : ComboBoxConfig
    {
        public EnumBoxConfig(Type enumType)
        {
            this.queryMode = "local";
            this.displayField = "text";
            this.valueField = "text";
            var models = EnumViewModel.GetByEnumType(enumType);
            this.store = new ArrayStoreConfig
            {
                fields = new string[] { "text", "value" },
                data = models.Select(vm => new EnumModel
                {
                    text = vm.Label,
                    value = (int)(object)vm.EnumValue
                }).ToArray()
            };
        }
    }

    internal class EnumModel : JsonModel
    {
        public string text { get; set; }
        public int value { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("text", text);
            json.WriteProperty("value", value, true);
        }
    }
}