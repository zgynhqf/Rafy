using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;

namespace $domainNamespace$
{
    public class $domainName$Plugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            app.RuntimeStarting += (o, e) => AutoUpdateDb();
        }

        /// <summary>
        /// 自动升级数据库。
        /// </summary>
        private static void AutoUpdateDb()
        {
            var svc = ServiceFactory.Create<MigrateService>();
            svc.Options = new MigratingOptions
            {
                //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
                RunDataLossOperation = DataLossOperation.All,//要支持数据库表、字段的删除操作，取消本行注释。
                Databases = new string[] { $domainName$EntityRepositoryDataProvider.DbSettingName }
            };
            svc.Invoke();
        }
    }
}
