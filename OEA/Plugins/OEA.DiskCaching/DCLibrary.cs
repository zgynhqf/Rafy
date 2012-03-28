using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library.ORM.DbMigration;
using OEA.Library.Caching;

namespace OEA.ClientCachingProvider
{
    internal class DCLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
            EntityListVersion.SetProvider(new EntityListVersionRepository());

            //服务端在启动时，需要清空缓存信息库，这会导致所有客户端都重新下载最新的数据。
            if (OEAEnvironment.Location == OEALocation.WPFServer)
            {
                app.StartupCompleted += (o, e) => { EntityListVersion.Repository.Clear(); };
            }

            app.DbMigratingOperations += (o, e) =>
            {
                using (var c = new OEADbMigrationContext(ConnectionStringNames.OEA))
                {
                    //由于其它的库可能需要在 OEA 库中添加表，所以这里
                    c.RunDataLossOperation = false;

                    //c.RollbackToHistory(DateTime.Parse("2008-12-31 23:59:58.700"), RollbackAction.DeleteHistory);
                    c.AutoMigrate();
                };
            };
        }
    }
}