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
    /// 多关联 关系
    /// </summary>
    public class HasManyAssociation : EntityAssociation
    {
        public HasManyAssociation()
        {
            //this.autoLoad = true;
        }

        public string name { get; set; }

        public string foreignKey { get; set; }

        public bool autoLoad { get; set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("type", "hasMany");
            json.WritePropertyIf("name", name);
            json.WriteProperty("primaryKey", "Id");
            json.WritePropertyIf("foreignKey", foreignKey);

            if (autoLoad) { json.WriteProperty("autoLoad", true); }

            base.ToJson(json);
        }
    }
}