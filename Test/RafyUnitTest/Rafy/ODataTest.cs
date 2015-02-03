/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20141217
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20141217 17:20
 * 
*******************************************************/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class ODataTest
    {
        [ClassInitialize]
        public static void ODataTest_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void ODT_Filter()
        {
            var filter = "Name eq 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "huqf");
        }

        [TestMethod]
        public void ODT_Filter_ne()
        {
            var filter = "Name ne 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName != {0}");
        }

        [TestMethod]
        public void ODT_Filter_le()
        {
            var filter = "Name le 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName <= {0}");
        }

        [TestMethod]
        public void ODT_Filter_lt()
        {
            var filter = "Name lt 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName < {0}");
        }

        [TestMethod]
        public void ODT_Filter_gt()
        {
            var filter = "Name gt 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName > {0}");
        }

        [TestMethod]
        public void ODT_Filter_ge()
        {
            var filter = "Name ge 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName >= {0}");
        }

        [TestMethod]
        public void ODT_Filter_startwith()
        {
            var filter = "Name startwith 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName LIKE {0}");
            Assert.IsTrue(sql.Parameters[0].ToString() == "huqf%");
        }

        [TestMethod]
        public void ODT_Filter_endwith()
        {
            var filter = "Name endwith 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName LIKE {0}");
            Assert.IsTrue(sql.Parameters[0].ToString() == "%huqf");
        }

        [TestMethod]
        public void ODT_Filter_contains()
        {
            var filter = "Name contains 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName LIKE {0}");
            Assert.IsTrue(sql.Parameters[0].ToString() == "%huqf%");
        }

        [TestMethod]
        public void ODT_Filter_And()
        {
            var filter = "Name eq 'huqf' and LoginName eq 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0} AND Users.LoginName = {1}");
            Assert.IsTrue(sql.Parameters.Count == 2);
            Assert.IsTrue(sql.Parameters[0].ToString() == "huqf");
            Assert.IsTrue(sql.Parameters[1].ToString() == "huqf");
        }

        [TestMethod]
        public void ODT_Filter_Or()
        {
            var filter = "Name eq 'huqf' or LoginName eq 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0} OR Users.LoginName = {1}");
            Assert.IsTrue(sql.Parameters.Count == 2);
            Assert.IsTrue(sql.Parameters[0].ToString() == "huqf");
            Assert.IsTrue(sql.Parameters[1].ToString() == "huqf");
        }

        /// <summary>
        /// 注意，Or 与 And 没有优先级之分。
        /// </summary>
        [TestMethod]
        public void ODT_Filter_OrAnd()
        {
            var filter = "Name eq 'huqf' or LoginName eq 'huqf' and AddedTime lt '2014-12-17 19:00'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"(Users.UserName = {0} OR Users.LoginName = {1}) AND Users.AddedTime < {2}");
            Assert.IsTrue(sql.Parameters.Count == 3);
            Assert.IsTrue(sql.Parameters[0].ToString() == "huqf");
            Assert.IsTrue(sql.Parameters[1].ToString() == "huqf");
        }

        [TestMethod]
        public void ODT_Filter_AndOr()
        {
            var filter = "Name eq 'huqf' and LoginName eq 'huqf' or AddedTime lt '2014-12-17 19:00'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0} AND Users.LoginName = {1} OR Users.AddedTime < {2}");
        }

        [TestMethod]
        public void ODT_Filter_Bracket()
        {
            var filter = "AddedTime lt '2014-12-17 19:00' and (Name eq 'huqf' or LoginName eq 'huqf')";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.AddedTime < {0} AND (Users.UserName = {1} OR Users.LoginName = {2})");
        }

        [TestMethod]
        public void ODT_Filter_Bracket_Double()
        {
            var filter = "(Name eq 'huqf' or LoginName eq 'huqf') and (Age eq 10 or AddedTime lt '2014-12-17 19:00')";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"(Users.UserName = {0} OR Users.LoginName = {1}) AND (Users.Age = {2} OR Users.AddedTime < {3})");
        }

        [TestMethod]
        public void ODT_Filter_Bracket_Redundancy()
        {
            var filter = "(Name eq 'huqf' or LoginName eq 'huqf')";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0} OR Users.LoginName = {1}");
        }

        [TestMethod]
        public void ODT_Filter_Bracket_Redundancy2()
        {
            var filter = "(Name eq 'huqf')";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.UserName = {0}");
        }

        [TestMethod]
        public void ODT_Filter_Bracket_Redundancy_Multi()
        {
            var filter = "(AddedTime lt '2014-12-17 19:00') and (((Name eq 'huqf' or LoginName eq 'huqf')))";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"Users.AddedTime < {0} AND (Users.UserName = {1} OR Users.LoginName = {2})");
        }

        [TestMethod]
        public void ODT_Filter_Bracket_Complicate()
        {
            var filter = "(Name eq 'huqf' or (Age eq 10 or AddedTime lt '2014-12-17 19:00')) and LoginName eq 'huqf'";

            var sql = ParseWhere(filter);

            Assert.IsTrue(sql.ToString() == @"(Users.UserName = {0} OR Users.Age = {1} OR Users.AddedTime < {2}) AND Users.LoginName = {3}");
        }

        /// <summary>
        /// 引用属性也可以使用在 Filter 中。
        /// </summary>
        [TestMethod]
        public void ODT_Filter_ReferenceProperty()
        {
            var filter = "TestUser.Name contains 'huqf'";

            var sql = ParseFull(filter, RF.Concrete<TestRoleRepository>());

            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM Roles
    INNER JOIN Users AS T0 ON Roles.UserId = T0.Id
WHERE T0.UserName LIKE {0}");
            Assert.IsTrue(sql.Parameters[0].ToString() == "%huqf%");
        }

        /// <summary>
        /// 引用属性也可以使用在 Filter 中。
        /// </summary>
        [TestMethod]
        public void ODT_Filter_ReferenceProperty_Complicated()
        {
            var filter = "(Name eq 'huqf' or (TestUser.Age eq 10 or TestUser.AddedTime lt '2014-12-17 19:00')) and TestUser.Name contains 'huqf'";

            var sql = ParseFull(filter, RF.Concrete<TestRoleRepository>());

            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM Roles
    INNER JOIN Users AS T0 ON Roles.UserId = T0.Id
WHERE (Roles.Name = {0} OR T0.Age = {1} OR T0.AddedTime < {2}) AND T0.UserName LIKE {3}");
        }

        [TestMethod]
        public void ODT_ODataQuery_OrderBy()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new TestUser { Name = "1" });
                RF.Save(new TestUser { Name = "2" });
                RF.Save(new TestUser { Name = "3" });

                var list = QueryUserList(new ODataQueryCriteria
                {
                    OrderBy = "Name"
                });

                Assert.AreEqual(list.Count, 3);
                Assert.IsTrue(string.Compare(list[0].Name, list[1].Name) == -1);
                Assert.IsTrue(string.Compare(list[1].Name, list[2].Name) == -1);
            }
        }

        [TestMethod]
        public void ODT_ODataQuery_OrderBy2()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new TestUser { Name = "1" });
                RF.Save(new TestUser { Name = "2" });
                RF.Save(new TestUser { Name = "3" });

                var list = QueryUserList(new ODataQueryCriteria
                {
                    OrderBy = "Name asc"
                });

                Assert.AreEqual(list.Count, 3);
                Assert.IsTrue(string.Compare(list[0].Name, list[1].Name) == -1);
                Assert.IsTrue(string.Compare(list[1].Name, list[2].Name) == -1);
            }
        }

        [TestMethod]
        public void ODT_ODataQuery_OrderBy_Decending()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new TestUser { Name = "1" });
                RF.Save(new TestUser { Name = "2" });
                RF.Save(new TestUser { Name = "3" });

                var list = QueryUserList(new ODataQueryCriteria
                {
                    OrderBy = "Name desc"
                });

                Assert.AreEqual(list.Count, 3);
                Assert.IsTrue(string.Compare(list[0].Name, list[1].Name) == 1);
                Assert.IsTrue(string.Compare(list[1].Name, list[2].Name) == 1);
            }
        }

        [TestMethod]
        public void ODT_EagerLoad()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new Book
                {
                    Name = "book",
                    ChapterList =
                    {
                        new Chapter
                        {
                            Name = "chapter",
                            SectionList = 
                            {
                                new Section { Name = "Section1" },
                                new Section { Name = "Section2" },
                            }
                        }
                    }
                });

                var list = RF.Concrete<SectionRepository>().GetBy(new ODataQueryCriteria
                {
                    Expand = "Chapter"
                }) as SectionList;

                Assert.AreEqual(list.Count, 2);
                Assert.IsTrue(list[0].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[1].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[0].Chapter == list[1].Chapter);
            }
        }

        [TestMethod]
        public void ODT_EagerLoad_Multi()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var so = new SectionOwner { Name = "SO1" };
                RF.Save(so);
                RF.Save(new Book
                {
                    Name = "book",
                    ChapterList =
                    {
                        new Chapter
                        {
                            Name = "chapter",
                            SectionList = 
                            {
                                new Section { Name = "Section1", SectionOwner = so },
                                new Section { Name = "Section2", SectionOwner = so },
                            }
                        }
                    }
                });

                var list = RF.Concrete<SectionRepository>().GetBy(new ODataQueryCriteria
                {
                    Expand = "Chapter,SectionOwner"
                }) as SectionList;

                Assert.AreEqual(list.Count, 2);
                Assert.IsTrue(list[0].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[1].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[0].Chapter == list[1].Chapter);
                Assert.IsTrue(list[0].FieldExists(Section.SectionOwnerProperty));
                Assert.IsTrue(list[1].FieldExists(Section.SectionOwnerProperty));
                Assert.IsTrue(list[0].SectionOwner == list[1].SectionOwner);
            }
        }

        [TestMethod]
        public void ODT_EagerLoad_Cascade()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new Book
                {
                    Name = "book",
                    ChapterList =
                    {
                        new Chapter
                        {
                            Name = "chapter",
                            SectionList = 
                            {
                                new Section { Name = "Section1" },
                                new Section { Name = "Section2" },
                            }
                        }
                    }
                });

                var list = RF.Concrete<SectionRepository>().GetBy(new ODataQueryCriteria
                {
                    Expand = "Chapter.Book"
                }) as SectionList;

                Assert.AreEqual(list.Count, 2);
                Assert.IsTrue(list[0].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[1].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[0].Chapter == list[1].Chapter);
                Assert.IsTrue(list[0].Chapter.FieldExists(Chapter.BookProperty), "Chapter.Book 也已经加载");
            }
        }

        [TestMethod]
        public void ODT_EagerLoad_CascadeAndMulti()
        {
            var so = new SectionOwner { Name = "SO1" };
            RF.Save(so);
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                RF.Save(new Book
                {
                    Name = "book",
                    ChapterList =
                    {
                        new Chapter
                        {
                            Name = "chapter",
                            SectionList = 
                            {
                                new Section { Name = "Section1", SectionOwner = so },
                                new Section { Name = "Section2", SectionOwner = so },
                            }
                        }
                    }
                });

                var list = RF.Concrete<SectionRepository>().GetBy(new ODataQueryCriteria
                {
                    Expand = "Chapter.Book,SectionOwner"
                }) as SectionList;

                Assert.AreEqual(list.Count, 2);
                Assert.IsTrue(list[0].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[1].FieldExists(Section.ChapterProperty));
                Assert.IsTrue(list[0].Chapter == list[1].Chapter);
                Assert.IsTrue(list[0].FieldExists(Section.SectionOwnerProperty));
                Assert.IsTrue(list[1].FieldExists(Section.SectionOwnerProperty));
                Assert.IsTrue(list[0].SectionOwner == list[1].SectionOwner);
                Assert.IsTrue(list[0].Chapter.FieldExists(Chapter.BookProperty), "Chapter.Book 也已经加载");
            }
        }

        private static FormattedSql ParseWhere(string filter)
        {
            var repo = RF.Concrete<TestUserRepository>();
            var q = Parse(filter, repo);
            return QueryNodeTester.GenerateTestSql(q.Where);
        }

        private static FormattedSql ParseFull(string filter, IRepository repo)
        {
            var q = Parse(filter, repo);
            return QueryNodeTester.GenerateTestSql(q);
        }

        private static IQuery Parse(string filter, IRepository repo)
        {
            var f = QueryFactory.Instance;
            //var t = f.Table(repo, "T0");
            var q = f.Query(repo);
            var parser = new ODataFilterParser
            {
                _properties = repo.EntityMeta.ManagedProperties.GetCompiledProperties()
            };

            parser.Parse(filter, q);
            return q;
        }

        private static TestUserList QueryUserList(ODataQueryCriteria criteria)
        {
            return RF.Concrete<TestUserRepository>().GetBy(criteria) as TestUserList;
        }
    }
}
