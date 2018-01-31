/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 15:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Accounts;
using Rafy.Accounts.Controllers;
using Rafy.DataArchiver;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Stamp;
using Rafy.ManagedProperty;
using Rafy.UnitTest.Repository;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class DataArchiverTest_BatchImport : DataArchiverTest
    {
        [ClassInitialize]
        public static void DAT_BI_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        protected override AggregationArchiver CreateAggregationArchiver()
        {
            return new BatchImportAggregationArchiver();
        }
    }

    public class BatchImportAggregationArchiver : AggregationArchiver
    {
        protected override void SaveList(IRepository repository, EntityList entityList)
        {
            repository.CreateImporter().Save(entityList);
        }
    }
}