using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC;
using Rafy.WPF;

namespace JXC
{
    class JXCPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            //app.DbMigratingOperations += (s, e) =>
            //{
            //    using (var db = new RafyDbMigrationContext(JXCEntity.ConnectionString))
            //    {
            //        db.RollbackAll(RollbackAction.DeleteHistory);
            //        db.ResetDbVersion();

            //        db.AutoMigrate();
            //    }
            //};

            app.StartupCompleted += app_StartupCompleted;
        }

        private void app_StartupCompleted(object sender, EventArgs e)
        {
            //var __watch = new System.Diagnostics.Stopwatch();
            //__watch.Start();
            //var repo = RF.Concreate<AuditItemRepository>();
            //using (var tran = RF.TransactionScope(repo))
            //{
            //    var list = new AuditItemList();

            //    for (int i = 21; i <= 30; i++)
            //    {
            //        var date = new DateTime(2012, 09, i);
            //        for (int j = 0; j < 1000; j++)
            //        {
            //            var item = new AuditItem
            //            {
            //                Content = i.ToString(),
            //                LogTime = date,
            //            };
            //            list.Add(item);
            //        }
            //    }
            //    repo.Save(list);
            //    tran.Complete();
            //}
            //__watch.Stop();
            //var __usedToBreak = __watch.Elapsed;//打断点在这一行查看耗时
            //System.IO.File.WriteAllText(@"D:\InsertAll 耗时：" + __usedToBreak.ToString().Replace(":", "-"), "1");
        }
    }
}