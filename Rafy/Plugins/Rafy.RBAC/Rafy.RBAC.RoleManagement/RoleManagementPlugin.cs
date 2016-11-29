using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;

namespace Rafy.RBAC.RoleManagement
{
    public class RoleManagementPlugin : DomainPlugin
    {
        public static string DbSettingName = "RoleManagement";

        public override void Initialize(IApp app)
        {
        }
    }
}
