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
    /// 客户端的实体模型定义
    /// </summary>
    public class EntityModel : JsonModel
    {
        public EntityModel()
        {
            this.fields = new List<EntityField>();
            this.associations = new List<EntityAssociation>();
        }

        /// <summary>
        /// 实体是否是树型对象。
        /// </summary>
        public bool isTree { get; set; }

        /// <summary>
        /// 实体字段
        /// </summary>
        public IList<EntityField> fields { get; private set; }

        /// <summary>
        /// 实体关系
        /// </summary>
        public IList<EntityAssociation> associations { get; private set; }

        protected override void ToJson(LiteJsonWriter json)
        {
            if (isTree)
            {
                json.WritePropertyIf("isTree", isTree);
            }

            json.WriteProperty("fields", fields);

            if (associations.Count > 0)
            {
                json.WriteProperty("associations", associations);
            }
        }
    }
}