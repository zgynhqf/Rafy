/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 02:16
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.FileStorage;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.BatchSubmit;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.Serialization;
using Rafy.Domain.Serialization.Json;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.Reflection;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class FileStoragePluginTest
    {
        [ClassInitialize]
        public static void FSPT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);

            RafyEnvironment.LoadPlugin(typeof(FileStorageEntity).Assembly);
        }

        [TestMethod]
        public void FSPT_Save()
        {
            var repo = RF.ResolveInstance<FileInfoRepository>();
            using (RF.TransactionScope(repo))
            {
                var file = new FileInfo
                {
                    Name = "1.txt",
                    Description = "Nothing",
                    Content = Encoding.UTF8.GetBytes("File Content")
                };
                repo.Save(file);

                var file2 = repo.GetById(file.Id);
                Assert.IsNotNull(file2, "已经正确存储到数据库中");
                Assert.IsNotNull(file2.Storage);
                Assert.IsTrue(System.IO.File.Exists(file2.Storage));
                Assert.AreEqual("1.txt", file2.Name);
                Assert.AreEqual("Nothing", file2.Description);
            }
        }

        [TestMethod]
        public void FSPT_ReadContent_LazyLoad()
        {
            var repo = RF.ResolveInstance<FileInfoRepository>();
            using (RF.TransactionScope(repo))
            {
                var file = new FileInfo
                {
                    Content = Encoding.UTF8.GetBytes("File Content")
                };
                repo.Save(file);

                var file2 = repo.GetById(file.Id);

                Assert.IsFalse(file2.FieldExists(FileInfo.ContentProperty), "该属性需要懒加载");
            }
        }

        [TestMethod]
        public void FSPT_ReadContent()
        {
            var repo = RF.ResolveInstance<FileInfoRepository>();
            using (RF.TransactionScope(repo))
            {
                var file = new FileInfo
                {
                    Content = Encoding.UTF8.GetBytes("File Content")
                };
                repo.Save(file);

                var file2 = repo.GetById(file.Id);

                Assert.IsNotNull(file2.Content);

                var content = Encoding.UTF8.GetString(file2.Content);
                Assert.AreEqual("File Content", content);
            }
        }

        [TestMethod]
        public void FSPT_UpdateContent()
        {
            var repo = RF.ResolveInstance<FileInfoRepository>();
            using (RF.TransactionScope(repo))
            {
                var file = new FileInfo
                {
                    Content = Encoding.UTF8.GetBytes("File Content")
                };
                repo.Save(file);

                var file2 = repo.GetById(file.Id);
                file2.Content = Encoding.UTF8.GetBytes("File Content 2");
                repo.Save(file2);

                var file3 = repo.GetById(file.Id);

                Assert.IsNotNull(file3.Content);

                var content = Encoding.UTF8.GetString(file3.Content);
                Assert.AreEqual("File Content 2", content);
            }
        }
    }
}
