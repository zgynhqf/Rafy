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
    public class AuditServerService : SimpleCsla.ServiceBase
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
        protected override void DataPortal_Execute()
        {
            //server log
            AuditLogService.Log(this._logItem);
        }
    }
}
