using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.Audit
{
    /// <summary>
    /// 客户端使用这个类来和服务端通信
    /// </summary>
    [Serializable]
    public class AuditServerService : Service
    {
        private AuditLogItem _logItem;

        public AuditServerService(AuditLogItem logItem)
        {
            if (logItem == null) throw new ArgumentNullException("logItem");

            this._logItem = logItem;
        }

        /// <summary>
        /// 调用服务端Provider对log进行记录。
        /// </summary>
        protected override void Execute()
        {
            //server log
            AuditLogService.Log(this._logItem);
        }
    }
}
