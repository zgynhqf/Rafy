﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Utils;

namespace OEA.Library.Audit
{
    [Serializable]
    public class ClearLogService : Service
    {
        protected override void Execute()
        {
            using (var db = DBHelper.CreateDb(ConnectionStringNames.OEAPlugins))
            {
                db.DBA.ExecuteTextNormal("DELETE FROM AUDITITEM");
            }
        }
    }
}