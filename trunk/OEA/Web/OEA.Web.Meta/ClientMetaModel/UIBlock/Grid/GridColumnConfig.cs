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
    /// 列表中某一列的配置
    /// </summary>
    public class GridColumnConfig : JsonModel
    {
        public string header { get; set; }

        public string dataIndex { get; set; }

        public int flex { get; set; }

        public bool locked { get; set; }

        public string xtype { get; set; }

        public FieldConfig editor { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("header", header);
            json.WritePropertyIf("xtype", xtype);
            json.WritePropertyIf("editor", editor);
            if (locked) { json.WritePropertyIf("locked", locked); }

            if (flex > 0)
            {
                json.WriteProperty("flex", flex);
            }
            json.WriteProperty("dataIndex", dataIndex, true);
        }
    }
}