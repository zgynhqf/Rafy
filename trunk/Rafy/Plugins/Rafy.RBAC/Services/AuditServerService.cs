using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.RBAC.Audit
{
    /// <summary>
    /// 客户端使用这个类来和服务端通信
    /// </summary>
    [Serializable]
    [Contract, ContractImpl]
    public class AuditServerService : Service
    {
        public AuditLogItem LogItem { get; set; }

        /// <summary>
        /// 调用服务端Provider对log进行记录。
        /// </summary>
        protected override void Execute()
        {
            if (LogItem == null) throw new ArgumentNullException("LogItem");

            //server log
            AuditLogService.Log(this.LogItem);
        }
    }
}
