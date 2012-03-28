using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Audit;
using System.Threading;
using OEA.Library.Audit;

namespace OEA.Library.Audit
{
    public class ServerAuditLogProvider : IAuditLogProvider
    {
        #region IAuditLogProvider Members

        public void Log(AuditLogItem log)
        {
            var dbItem = RF.NewEntity<AuditItem>();

            dbItem.Title = log.Title;
            dbItem.Content = log.FriendlyContent;
            dbItem.PrivateContent = log.PrivateContent;
            dbItem.User = log.User;
            dbItem.MachineName = log.MachineName;
            dbItem.ModuleName = log.ModuleName;
            dbItem.Type = log.Type;
            dbItem.LogTime = log.LogTime;
            dbItem.EntityId = log.EntityId;

            dbItem.Save();
        }

        #endregion
    }
}
