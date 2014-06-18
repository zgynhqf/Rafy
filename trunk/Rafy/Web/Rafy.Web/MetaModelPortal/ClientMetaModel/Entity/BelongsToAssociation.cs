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
    /// 多对一关联关系字义
    /// </summary>
    public class BelongsToAssociation : EntityAssociation
    {
        public string associationKey;

        public string foreignKey;

        protected override void ToJson(LiteJsonWriter json)
        {
            json.WriteProperty("type", "belongsTo");
            json.WritePropertyIf("foreignKey", foreignKey);
            json.WriteProperty("primaryKey", "Id");
            json.WritePropertyIf("associationKey", associationKey);

            base.ToJson(json);
        }
    }
}