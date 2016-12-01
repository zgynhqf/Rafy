/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120508
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120508
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using System.Transactions;
using Rafy.Domain;
using Rafy.Web;

namespace JXC
{
    [Serializable]
    [JsonService]
    [Contract, ContractImpl]
    public class AutoCodeService : Service
    {
        [ServiceOutput]
        public string code { get; set; }

        [ServiceInput]
        public string EntityType { get; set; }

        protected override void Execute()
        {
            if (!RafyEnvironment.Location.IsWebUI) throw new InvalidOperationException("此命令只在 Web 模式下可用。");

            var entityType = ClientEntities.Find(this.EntityType).EntityType;

            this.code = RF.ResolveInstance<AutoCodeInfoRepository>().GetOrCreateAutoCode(entityType);
        }
    }
}