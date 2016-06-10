using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using UT;

namespace Rafy.UnitTest
{
    public class UnitTestPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            app.RuntimeStarting += UnitTestPlugin_RuntimeStarting;
        }

        private void UnitTestPlugin_RuntimeStarting(object sender, EventArgs e)
        {
            var dbSetting = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            if (DbSetting.IsOracleProvider(dbSetting))
            {
                Rafy.Domain.ORM.BatchSubmit.Oracle.OracleBatchImporter.EnableBatchSequence(
                    RF.Concrete<BookRepository>()
                    );
            }
        }
    }
}