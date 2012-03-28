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
    /// 列表/树型列表的配置
    /// </summary>
    public class GridConfig : JsonModel
    {
        public GridConfig()
        {
            this.columns = new List<GridColumnConfig>();
            this.tbar = new List<ToolbarItem>();
        }

        /// <summary>
        /// 工具栏配置项
        /// </summary>
        public IList<ToolbarItem> tbar { get; private set; }

        /// <summary>
        /// 列配置
        /// </summary>
        public IList<GridColumnConfig> columns { get; private set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            if (tbar.Count > 0) { json.WriteProperty("tbar", tbar); }

            json.WriteProperty("columns", columns, true);
        }
    }
}