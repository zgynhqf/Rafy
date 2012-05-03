using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Utils;
using OEA.ORM;

namespace OEA.Library.Audit
{
    [Serializable]
    public class ClearLogService : Service
    {
        protected override void Execute()
        {
            using (var db = Db.Create(ConnectionStringNames.OEAPlugins))
            {
                db.DBA.ExecuteTextNormal("DELETE FROM AUDITITEM");
            }
        }
    }
}