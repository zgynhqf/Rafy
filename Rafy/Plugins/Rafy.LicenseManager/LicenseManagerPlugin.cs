using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using Rafy.LicenseManager.Entities;

namespace Rafy.LicenseManager
{
    public class LicenseManagerPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            app.RuntimeStarting += this._App_RuntimeStarting;
        }

        private void _App_RuntimeStarting(object sender, EventArgs e)
        {
            var svc = ServiceFactory.Create<MigrateService>();
            svc.Options = new MigratingOptions
            {
                //ReserveHistory = true,//ReserveHistory 表示是否需要保存所有数据库升级的历史记录
                RunDataLossOperation = DataLossOperation.All,//要支持数据库表、字段的删除操作，取消本行注释。
                Databases = new string[] { LicenseManagerEntityRepository.DbSettingName }
            };
            svc.Invoke();
        }
    }
}
