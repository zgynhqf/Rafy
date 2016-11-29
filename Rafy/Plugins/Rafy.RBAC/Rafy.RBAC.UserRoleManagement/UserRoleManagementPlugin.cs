using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;

namespace Rafy.RBAC.UserRoleManagement
{
    public class UserRoleManagementPlugin : DomainPlugin
    {
        public static string DbSettingName = "UserRoleManagement";

        public override void Initialize(IApp app)
        {
        }
    }
}
