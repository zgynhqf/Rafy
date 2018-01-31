using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM.BatchSubmit.Oracle;
using UT;

namespace Rafy.UnitTest
{
    public class UnitTestPlugin : DomainPlugin
    {
        public override void Initialize(IApp app)
        {
            //app.RuntimeStarting += UnitTestPlugin_RuntimeStarting;
        }

        //private void UnitTestPlugin_RuntimeStarting(object sender, EventArgs e)
        //{
        //    InitailizeSequences();
        //}

        public static void InitailizeSequences()
        {
            var dbSetting = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            if (DbSetting.IsOracleProvider(dbSetting))
            {
                OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<BookCategoryRepository>());
                OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<FolderRepository>());
                OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<BookRepository>());
                OracleBatchImporter.EnableBatchSequence(RF.ResolveInstance<InvoiceRepository>());
            }
        }
    }
}