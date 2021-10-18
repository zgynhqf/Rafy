/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160123
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160123 19:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
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
    public class MiscellaneousTest
    {
        [ClassInitialize]
        public static void MT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region Transaction

        [TestMethod]
        public void MT_Transaction_GetCurrentTransactionBlock()
        {
            using (var tranOuter = RF.TransactionScope(DbSettingNames.RafyPlugins))
            {
                using (var tranInner1 = RF.TransactionScope(DbSettingNames.RafyPlugins))
                {
                    var currentScope = LocalTransactionBlock.GetCurrentTransactionBlock(DbSetting.FindOrCreate(DbSettingNames.RafyPlugins));
                    Assert.AreEqual(currentScope, tranInner1);
                }
            }
        }

        [TestMethod]
        public void MT_Transaction_GetCurrentTransactionBlock2()
        {
            using (var tranOuter = RF.TransactionScope(DbSettingNames.RafyPlugins))
            {
                using (var tranInner1 = RF.TransactionScope(DbSettingNames.RafyPlugins))
                {
                    using (var tranInner2 = RF.TransactionScope(DbSettingNames.RafyPlugins))
                    {
                        var currentScope = LocalTransactionBlock.GetCurrentTransactionBlock(DbSetting.FindOrCreate(DbSettingNames.RafyPlugins));
                        Assert.AreEqual(currentScope, tranInner2);
                    }
                }
            }
        }

        #endregion

        #region Serialization

        [TestMethod]
        public void MT_Serialization_LoadOptions()
        {
            var elo = new LoadOptions();
            elo.LoadWith(Book.ChapterListProperty);
            elo.LoadWith(Chapter.SectionListProperty);
            elo.LoadWith(Section.SectionOwnerProperty);

            var elo2 = ObjectCloner.Clone(elo);

            Assert.AreEqual(elo.CoreList.Count, 3);
            Assert.AreEqual(elo.CoreList[0].Property, Book.ChapterListProperty);
            Assert.AreEqual(elo.CoreList[1].Property, Chapter.SectionListProperty);
            Assert.AreEqual(elo.CoreList[2].Property, Section.SectionOwnerProperty);
        }

        #endregion
    }
}
