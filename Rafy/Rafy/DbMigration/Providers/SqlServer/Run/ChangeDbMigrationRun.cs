using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Rafy.Data;

namespace Rafy.DbMigration.SqlServer.Run
{
    [DebuggerDisplay("Change To Database : {TargetDb}")]
    public class ChangeDbMigrationRun : MigrationRun
    {
        public string TargetDb { get; set; }

        protected override void RunCore(IDbAccesser db)
        {
            db.RawAccesser.ExecuteText("USE " + this.TargetDb);

            //var con = db.Connection;

            //var state = con.State;
            //if (state == ConnectionState.Closed) { con.Open(); }

            //db.Connection.ChangeDatabase(this.TargetDb);

            //if (state == ConnectionState.Closed) { con.Close(); }
        }
    }
}