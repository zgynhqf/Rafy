using Rafy;
using Rafy.ComponentModel;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy.Domain.ORM.DbMigration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.UnitTest.RuntimeLoad
{
    public class RuntimeLoadPlugin : DomainPlugin
    {
        public static string DbSettingName = "Test_RafyUnitTest";

        public override void Initialize(IApp app)
        {
        }
    }
}
