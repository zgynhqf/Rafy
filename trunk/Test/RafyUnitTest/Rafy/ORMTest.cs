using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.ORM.Oracle;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;
using Rafy.Reflection;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class ORMTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region EntityContext

        [TestMethod]
        public void ORM_EntityContext_Query()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = new TestUser();
                repo.Save(user);

                var user2 = repo.GetById(user.Id);
                Assert.IsTrue(user != user2, "实体上下文块外，二者应该是不同的对象。");

                using (RF.EnterEntityContext())
                {
                    var user3 = repo.GetById(user.Id);
                    var user4 = repo.GetById(user.Id);
                    Assert.IsTrue(user3 == user4, "实体上下文块内，二者应该是同一个对象。");
                }
            }
        }

        [TestMethod]
        public void ORM_EntityContext_Insert()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            using (RF.EnterEntityContext())
            {
                var user = new TestUser();
                repo.Save(user);

                var user2 = repo.GetById(user.Id);
                Assert.IsTrue(user == user2, "实体上下文块内，二者应该是同一个对象。");
            }
        }

        [TestMethod]
        public void ORM_EntityContext_Update()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = new TestUser();
                repo.Save(user);

                using (RF.EnterEntityContext())
                {
                    user.MarkModifiedIfUnchanged();
                    repo.Save(user);

                    var user2 = repo.GetById(user.Id);
                    Assert.IsTrue(user == user2, "实体上下文块内，二者应该是同一个对象。");
                }
            }
        }

        #endregion

        #region Query

        [TestMethod]
        public void ORM_Query_EmptyPaging_SingletonSerialization()
        {
            var cloned = ObjectCloner.Clone(PagingInfo.Empty);
            Assert.IsTrue(cloned == PagingInfo.Empty, "EmptyPagingInfo 只有在单例情况下，才能使用它作为空的分页参数。");
        }

        [TestMethod]
        public void ORM_Query_OrderBy_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Name = "1" });
                repo.Save(new TestUser { Name = "2" });
                repo.Save(new TestUser { Name = "3" });

                var list = repo.GetByOrder(true);
                Assert.IsTrue((list[0] as TestUser).Name == "1", "排序出错");
                Assert.IsTrue((list[1] as TestUser).Name == "2", "排序出错");
                Assert.IsTrue((list[2] as TestUser).Name == "3", "排序出错");

                list = repo.GetByOrder(false);
                Assert.IsTrue((list[0] as TestUser).Name == "3", "排序出错");
                Assert.IsTrue((list[1] as TestUser).Name == "2", "排序出错");
                Assert.IsTrue((list[2] as TestUser).Name == "1", "排序出错");
            }
        }

        [TestMethod]
        public void ORM_Query_OrderBy()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Name = "1" });
                repo.Save(new TestUser { Name = "2" });
                repo.Save(new TestUser { Name = "3" });

                var list = repo.GetByOrder2(true);
                Assert.IsTrue((list[0] as TestUser).Name == "1", "排序出错");
                Assert.IsTrue((list[1] as TestUser).Name == "2", "排序出错");
                Assert.IsTrue((list[2] as TestUser).Name == "3", "排序出错");

                list = repo.GetByOrder2(false);
                Assert.IsTrue((list[0] as TestUser).Name == "3", "排序出错");
                Assert.IsTrue((list[1] as TestUser).Name == "2", "排序出错");
                Assert.IsTrue((list[2] as TestUser).Name == "1", "排序出错");
            }
        }

        [TestMethod]
        public void ORM_Query_DefaultOrderBy_Id_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                for (int i = 0; i < 10; i++) { repo.Save(new TestUser()); }

                var list = repo.GetByEmptyArgument();
                for (int i = 1, c = list.Count; i < c; i++)
                {
                    var item2 = list[i];
                    var item1 = list[i - 1];
                    Assert.IsTrue(item1.Id < item2.Id, "默认应该按照 Id 正序排列。");
                }
            }
        }

        [TestMethod]
        public void ORM_Query_DefaultOrderBy_Id()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                for (int i = 0; i < 10; i++) { repo.Save(new TestUser()); }

                var list = repo.GetByEmptyArgument2();
                for (int i = 1, c = list.Count; i < c; i++)
                {
                    var item2 = list[i];
                    var item1 = list[i - 1];
                    Assert.IsTrue(item1.Id < item2.Id, "默认应该按照 Id 正序排列。");
                }
            }
        }

        [TestMethod]
        public void ORM_Query_MatchCriteria_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByNameAge_PropertyQuery("user1", 1);
                Assert.IsTrue(list.Count == 1);

                list = repo.GetByNameAge_PropertyQuery("user2", 1);
                Assert.IsTrue(list.Count == 2);

                list = repo.GetByNameAge_PropertyQuery("user2", 2);
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ORM_Query_MatchCriteria()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByNameAge("user1", 1);
                Assert.IsTrue(list.Count == 1);

                list = repo.GetByNameAge("user2", 1);
                Assert.IsTrue(list.Count == 2);

                list = repo.GetByNameAge("user2", 2);
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ORM_Query_CommonQueryCriteria()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByNameOrAge("2", 2);
                Assert.IsTrue(list.Count == 2);

                list = repo.GetByNameOrAge("3", 1);
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_CommonQueryCriteria_getBy()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetBy(new CommonQueryCriteria
                {
                    new PropertyMatchGroup
                    {
                        new PropertyMatch(TestUser.NameProperty, PropertyOperator.Contains, "3")
                    },
                    new PropertyMatchGroup
                    {
                        new PropertyMatch(TestUser.AgeProperty, 2)
                    }
                });
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByNameAgeByMultiParameters("user1", 1);
                Assert.IsTrue(list.Count == 1);

                list = repo.GetByNameAgeByMultiParameters("user2", 1);
                Assert.IsTrue(list.Count == 2);

                list = repo.GetByNameAgeByMultiParameters("user2", 2);
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByNameAgeByMultiParameters2("user1", 1);
                Assert.IsTrue(list.Count == 1);

                list = repo.GetByNameAgeByMultiParameters2("user2", 1);
                Assert.IsTrue(list.Count == 2);

                list = repo.GetByNameAgeByMultiParameters2("user2", 2);
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_Null_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list2 = repo.GetByNameAgeByMultiParameters(null, 1);
                Assert.IsTrue(list2.Count == 3);

                var list = repo.GetByNameAgeByMultiParameters(string.Empty, 1);
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_Null()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list2 = repo.GetByNameAgeByMultiParameters2(null, 1);
                Assert.IsTrue(list2.Count == 3);

                var list = repo.GetByNameAgeByMultiParameters2(string.Empty, 1);
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_Empty_IPropertyQuery()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByEmptyArgument();
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_Empty()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user1" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });
                repo.Save(new TestUser { Age = 1, Name = "user2" });

                var list = repo.GetByEmptyArgument2();
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_IntArray()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var a1 = new TestUser { Age = 1, Name = "user1" };
                var a2 = new TestUser { Age = 1, Name = "user2" };
                repo.Save(a1);
                repo.Save(a2);
                repo.Save(new TestUser { Age = 1, Name = "user3" });

                var list = repo.GetByIds(new int[] { a1.Id, a2.Id });
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_ByMultiParameters_IntArrayParam()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var a1 = new TestUser { Age = 1, Name = "user1" };
                var a2 = new TestUser { Age = 1, Name = "user2" };
                repo.Save(a1);
                repo.Save(a2);
                repo.Save(new TestUser { Age = 1, Name = "user3" });

                var list = repo.GetByIds2("user", a1.Id, a2.Id);
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_EagerLoad_IPropertyQuery()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var so = new SectionOwner();
                RF.Save(so);

                var book = CreateAggtBook(so);
                RF.Save(book);

                //查询的数据访问测试。
                var oldCount = Logger.DbAccessedCount;
                var all = repo.GetWithEager();
                var newCount = Logger.DbAccessedCount;
                Assert.IsTrue(newCount - oldCount == 4, "应该只进行了 4 次数据库查询。");

                //无懒加载测试。
                foreach (Book book2 in all)
                {
                    foreach (Chapter chapter in book2.ChapterList)
                    {
                        foreach (Section section in chapter.SectionList)
                        {
                            var so2 = section.SectionOwner;
                        }
                    }
                }
                Assert.IsTrue(Logger.DbAccessedCount == newCount, "由于数据已经全部加载完成，所以这里不会发生懒加载。");
            }
        }

        /// <summary>
        /// 贪婪加载
        /// </summary>
        [TestMethod]
        public void ORM_Query_EagerLoad()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var so = new SectionOwner();
                RF.Save(so);

                var book = CreateAggtBook(so);
                RF.Save(book);

                //查询的数据访问测试。
                var oldCount = Logger.DbAccessedCount;
                var all = repo.GetWithEager2();
                var newCount = Logger.DbAccessedCount;
                Assert.IsTrue(newCount - oldCount == 4, "应该只进行了 4 次数据库查询。");

                //无懒加载测试。
                foreach (Book book2 in all)
                {
                    foreach (Chapter chapter in book2.ChapterList)
                    {
                        foreach (Section section in chapter.SectionList)
                        {
                            var so2 = section.SectionOwner;
                        }
                    }
                }
                Assert.IsTrue(Logger.DbAccessedCount == newCount, "由于数据已经全部加载完成，所以这里不会发生懒加载。");
            }
        }

        [TestMethod]
        public void ORM_Query_Count_IPropertyQuery()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                RF.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                        new Chapter { Name = "2.3"}
                    }
                });

                var count = repo.CountByBookName("1");
                Assert.IsTrue(count == 2);
                count = repo.CountByBookName("2");
                Assert.IsTrue(count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_Count()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                RF.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                        new Chapter { Name = "2.3"}
                    }
                });

                var count = repo.CountByBookName2("1");
                Assert.IsTrue(count == 2);
                count = repo.CountByBookName2("2");
                Assert.IsTrue(count == 3);
            }
        }

        [TestMethod]
        public void ORM_Query_CountAll()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });

                var c = repo.CountAll();
                Assert.IsTrue(c == 4);
            }
        }

        [TestMethod]
        public void ORM_Query_CountByParentId()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                };
                RF.Save(book);

                var c = repo.CountByParentId(book.Id);
                Assert.IsTrue(c == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_GetFirst()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1" });
                repo.Save(new Book { Name = "2" });

                var c = repo.GetFirst();
                Assert.IsTrue(c.Name == "1");
            }
        }

        [TestMethod]
        public void ORM_Query_GetAll()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1" });
                repo.Save(new Book { Name = "2" });

                var list = repo.GetAll();
                Assert.IsTrue(list.Count == 2);
                Assert.IsTrue(list[0].Name == "1");
                Assert.IsTrue(list[1].Name == "2");
            }
        }

        [TestMethod]
        public void ORM_Query_GetById()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1" };
                repo.Save(book);
                repo.Save(new Book { Name = "2" });

                var book2 = repo.GetById(book.Id);
                Assert.IsTrue(book2.Name == book.Name);
            }
        }

        [TestMethod]
        public void ORM_Query_GetByIdList()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1" };
                repo.Save(book);
                var book2 = new Book { Name = "2" };
                repo.Save(book2);
                repo.Save(new Book { Name = "3" });

                var list = repo.GetByIdList(new object[] { book.Id, book2.Id });
                Assert.IsTrue(list.Count == 2);
                Assert.IsTrue(list[0].Name == "1");
                Assert.IsTrue(list[1].Name == "2");
            }
        }

        [TestMethod]
        public void ORM_Query_GetByParentIdList()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    ChapterList =
                    {
                        new Chapter{ Name = "1.1" },
                        new Chapter{ Name = "1.2" },
                        new Chapter{ Name = "1.3" },
                    }
                };
                repo.Save(book);
                var book2 = new Book
                {
                    ChapterList =
                    {
                        new Chapter{ Name = "2.1" },
                        new Chapter{ Name = "2.2" },
                        new Chapter{ Name = "2.3" },
                    }
                };
                repo.Save(book2);
                var book3 = new Book
                {
                    ChapterList =
                    {
                        new Chapter{ Name = "3.1" },
                        new Chapter{ Name = "3.2" },
                        new Chapter{ Name = "3.3" },
                    }
                };
                repo.Save(book3);

                var list = RF.Concrete<ChapterRepository>().GetByParentIdList(new object[] { book.Id, book2.Id });
                Assert.IsTrue(list.Count == 6);
                Assert.IsTrue(list[0].Name == "1.1");
                Assert.IsTrue(list[1].Name == "1.2");
                Assert.IsTrue(list[2].Name == "1.3");
                Assert.IsTrue(list[3].Name == "2.1");
                Assert.IsTrue(list[4].Name == "2.2");
                Assert.IsTrue(list[5].Name == "2.3");
            }
        }

        [TestMethod]
        public void ORM_Query_GetByParentId()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    ChapterList =
                    {
                        new Chapter{ Name = "1" },
                        new Chapter{ Name = "2" },
                        new Chapter{ Name = "3" },
                    }
                };
                repo.Save(book);

                var list = RF.Concrete<ChapterRepository>().GetByParentId(book.Id);
                Assert.IsTrue(list.Count == 3);
                Assert.IsTrue(list[0].Name == "1");
                Assert.IsTrue(list[1].Name == "2");
                Assert.IsTrue(list[2].Name == "3");
            }
        }

        [TestMethod]
        public void ORM_Query_GetBy()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "123" });
                repo.Save(new Book { Name = "234" });
                repo.Save(new Book { Name = "345" });
                repo.Save(new Book { Name = "456" });

                var list = repo.GetBy(new BookContainesNameCriteria { Name = "3" });
                Assert.IsTrue(list.Count == 3);
                Assert.IsTrue((list[0] as Book).Name == "123");
                Assert.IsTrue((list[1] as Book).Name == "234");
                Assert.IsTrue((list[2] as Book).Name == "345");
            }
        }

        [TestMethod]
        public void ORM_Query_TwoPropertiesConstraint_IPropertyQuery()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Code = "1", Name = "2" });
                repo.Save(new Book { Code = "2", Name = "2" });
                repo.Save(new Book { Code = "3", Name = "1" });
                repo.Save(new Book { Code = "4", Name = "4" });

                var list = repo.Get_NameEqualsCode();
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_TwoPropertiesConstraint()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Code = "1", Name = "2" });
                repo.Save(new Book { Code = "2", Name = "2" });
                repo.Save(new Book { Code = "3", Name = "1" });
                repo.Save(new Book { Code = "4", Name = "4" });

                var list = repo.Get_NameEqualsCode2();
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_WhereChildrenExists()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { });
                repo.Save(new Book { });
                repo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                    }
                });

                var list = repo.GetIfChildrenExists();
                Assert.IsTrue(list.Count == 1);
                var book = list[0];
                Assert.IsTrue(book.Name == "1");
            }
        }

        [TestMethod]
        public void ORM_Query_WhereChildrenExistsChapterName()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { });
                repo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                    }
                });
                repo.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "2.1"},
                    }
                });
                repo.Save(new Book
                {
                    Name = "3",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                    }
                });

                var list = repo.GetIfChildrenExists("1.1");
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_Query_WhereChildrenAllChapterName()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { });
                repo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                    }
                });
                repo.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.1"},
                    }
                });
                repo.Save(new Book
                {
                    Name = "3",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                    }
                });

                var list = repo.GetIfChildrenAll("1.1");
                Assert.IsTrue(list.Count == 3);
                Assert.IsTrue(list[1].Name == "1");
                Assert.IsTrue(list[2].Name == "2");
            }
        }

        #endregion

        #region Linq Query

        [TestMethod]
        public void ORM_LinqQuery_Where()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType { Name = "1" });
                repo.Save(new PBSType { Name = "1" });
                repo.Save(new PBSType { Name = "2" });

                var list = repo.LinqBySingleName("1");
                Assert.IsTrue(list.Count == 2);
            }
        }

        /// <summary>
        /// 单元测试：Linq 比较时，值放在等号的右边
        /// </summary>
        [TestMethod]
        public void ORM_LinqQuery_Where_Reverse()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType { Name = "1" });
                repo.Save(new PBSType { Name = "1" });
                repo.Save(new PBSType { Name = "2" });

                var list = repo.LinqBySingleNameReverse("1");
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_And()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType { Code = "1", Name = "1" });
                repo.Save(new PBSType { Code = "2", Name = "1" });
                repo.Save(new PBSType { Code = "1", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });

                var list = repo.LinqByCodeAndName("1", "1", true);
                Assert.IsTrue(list.Count == 1);

                list = repo.LinqByCodeAndName("2", "2", true);
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_Or()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType { Code = "1", Name = "1" });
                repo.Save(new PBSType { Code = "2", Name = "1" });
                repo.Save(new PBSType { Code = "1", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });

                var list = repo.LinqByCodeAndName("1", "1", false);
                Assert.IsTrue(list.Count == 3);

                list = repo.LinqByCodeAndName("2", "2", false);
                Assert.IsTrue(list.Count == 4);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_WhereOnWhere()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new PBSType { Code = "1", Name = "1" });
                repo.Save(new PBSType { Code = "2", Name = "1" });
                repo.Save(new PBSType { Code = "1", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });
                repo.Save(new PBSType { Code = "2", Name = "2" });

                var list = repo.LinqByWhereOnWHere("1", "1");
                Assert.IsTrue(list.Count == 1);

                list = repo.LinqByWhereOnWHere("2", string.Empty);
                Assert.IsTrue(list.Count == 3);
            }
        }

        #region OrderBy, ThenBy

        [TestMethod]
        public void ORM_LinqQuery_OrderBy()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                Save5ItemsForOrder(repo);

                var list = repo.LinqOrderByCode(OrderDirection.Ascending);
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var a = list[i];
                    var b = list[i + 1];
                    var value = string.Compare(a.Code, b.Code);
                    Assert.IsTrue(value == -1, "应该已经按照 Code 进行了排序");
                }
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_OrderByDescending()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                Save5ItemsForOrder(repo);

                var list = repo.LinqOrderByCode(OrderDirection.Descending);
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var a = list[i];
                    var b = list[i + 1];
                    var value = string.Compare(a.Code, b.Code);
                    Assert.IsTrue(value == 1, "应该已经按照 Code 进行了排序");
                }
            }
        }

        private static void Save5ItemsForOrder(PBSTypeRepository repo)
        {
            repo.Save(new PBSType { Code = "3", Name = "5" });
            repo.Save(new PBSType { Code = "2", Name = "3" });
            repo.Save(new PBSType { Code = "1", Name = "4" });
            repo.Save(new PBSType { Code = "5", Name = "1" });
            repo.Save(new PBSType { Code = "4", Name = "2" });
        }

        [TestMethod]
        public void ORM_LinqQuery_OrderByThenBy()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                Save5ItemsForThenBy(repo);

                var list = repo.LinqOrderByCode(OrderDirection.Ascending, OrderDirection.Ascending);
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var a = list[i];
                    var b = list[i + 1];
                    var value = string.Compare(a.Name, b.Name);
                    Assert.IsTrue(value == -1, "应该已经按照 Name 进行了排序");
                }
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_OrderByThenByDescending()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                Save5ItemsForThenBy(repo);

                var list = repo.LinqOrderByCode(OrderDirection.Ascending, OrderDirection.Descending);
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var a = list[i];
                    var b = list[i + 1];
                    var value = string.Compare(a.Name, b.Name);
                    Assert.IsTrue(value == 1, "应该已经按照 Name 进行了排序");
                }
            }
        }

        private static void Save5ItemsForThenBy(PBSTypeRepository repo)
        {
            repo.Save(new PBSType { Code = "1", Name = "5" });
            repo.Save(new PBSType { Code = "1", Name = "3" });
            repo.Save(new PBSType { Code = "1", Name = "4" });
            repo.Save(new PBSType { Code = "1", Name = "1" });
            repo.Save(new PBSType { Code = "1", Name = "2" });
        }

        #endregion

        [TestMethod]
        public void ORM_LinqQuery_WhereRef()
        {
            var repo = RF.Concrete<ChapterRepository>();
            var rootRepo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                rootRepo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                rootRepo.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                        new Chapter { Name = "2.3"}
                    }
                });

                var list = repo.LinqGetByBookName("1");
                Assert.IsTrue(list.Count == 2);
                list = repo.LinqGetByBookName("2");
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_WhereRefAndWhereRef()
        {
            var repo = RF.Concrete<SectionRepository>();
            using (RF.TransactionScope(repo))
            {
                var sectionOwner = new SectionOwner { Name = "SO" };
                RF.Save(sectionOwner);

                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter 
                        { 
                            Name = "1.1",
                            SectionList = 
                            {
                                new Section { Name = "1.1.1", SectionOwner = sectionOwner },
                                new Section { Name = "1.1.2", SectionOwner = sectionOwner },
                                new Section { Name = "1.1.3" },
                            }
                        },
                        new Chapter 
                        { 
                            Name = "1.2",
                            SectionList = 
                            {
                                new Section { Name = "1.1.1", SectionOwner = sectionOwner },
                                new Section { Name = "1.1.2" },
                                new Section { Name = "1.1.3" },
                                new Section { Name = "1.1.4" },
                            }
                        },
                    }
                });
                RF.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter 
                        { 
                            Name = "2.1",
                            SectionList = 
                            {
                                new Section { Name = "2.1.1", SectionOwner = sectionOwner },
                                new Section { Name = "2.1.2", SectionOwner = sectionOwner },
                                new Section { Name = "2.1.3" },
                            }
                        },
                    }
                });

                var list = repo.GetByBookNameOwner("1", 1);
                Assert.IsTrue(list.Count == 3);
                list = repo.GetByBookNameOwner("1", 2);
                Assert.IsTrue(list.Count == 4);
                list = repo.GetByBookNameOwner("2", 0);
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_Count()
        {
            var repo = RF.Concrete<ChapterRepository>();
            var rootRepo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                rootRepo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                rootRepo.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                        new Chapter { Name = "2.3"}
                    }
                });

                var count = repo.LinqCountByBookName("1");
                Assert.IsTrue(count == 2);
                count = repo.LinqCountByBookName("2");
                Assert.IsTrue(count == 3);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_PureLinqCount()
        {
            var repo = RF.Concrete<ChapterRepository>();
            var rootRepo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                rootRepo.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"}
                    }
                });
                rootRepo.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                        new Chapter { Name = "2.3"}
                    }
                });

                var count = repo.LinqCountByBookName_RealLinqCount("1");
                Assert.IsTrue(count == 2);
                count = repo.LinqCountByBookName_RealLinqCount("2");
                Assert.IsTrue(count == 3);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_Paging()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"},
                        new Chapter { Name = "1.3"},
                        new Chapter { Name = "1.4"},
                        new Chapter { Name = "1.5"},
                        new Chapter { Name = "1.6"},
                        new Chapter { Name = "1.7"},
                        new Chapter { Name = "1.8"},
                        new Chapter { Name = "1.9"},
                    }
                });

                var pi = new PagingInfo(2, 3, true);
                var list = repo.LinqGetByBookNamePaging("1", pi);
                Assert.IsTrue(list.Count == 3);
                Assert.IsTrue(list[0].Name == "1.3");
                Assert.IsTrue(pi.TotalCount == 10);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_Paging_PageNumer1()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "1.2"},
                        new Chapter { Name = "1.3"},
                        new Chapter { Name = "1.4"},
                        new Chapter { Name = "1.5"},
                        new Chapter { Name = "1.6"},
                        new Chapter { Name = "1.7"},
                        new Chapter { Name = "1.8"},
                        new Chapter { Name = "1.9"},
                    }
                });

                var pi = new PagingInfo(1, 3, true);
                var list = repo.LinqGetByBookNamePaging("1", pi);
                Assert.IsTrue(list.Count == 3);
                Assert.IsTrue(list[0].Name == "1.0");
                Assert.IsTrue(pi.TotalCount == 10);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringContains()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.3"},
                    }
                });

                var list = repo.LinqGetByNameStringAction(StringAction.Contains, "1");
                Assert.IsTrue(list.Count == 3);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringStartWith()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.3"},
                    }
                });

                var list = repo.LinqGetByNameStringAction(StringAction.StartsWith, "1");
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringEndWith()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.3"},
                    }
                });

                var list = repo.LinqGetByNameStringAction(StringAction.EndsWith, "3");
                Assert.IsTrue(list.Count == 1);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringNotEmpty()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.0"},
                        new Chapter { Name = "1.1"},
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.3"},
                        new Chapter { Name = ""},
                    }
                });

                var list = repo.LinqGetByNameStringAction(StringAction.NotEmpty);
                Assert.IsTrue(list.Count == 4);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringContainsRef()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1.1",
                    ChapterList =
                    {
                        new Chapter(),
                        new Chapter(),
                    }
                });
                RF.Save(new Book
                {
                    Name = "2.1",
                    ChapterList =
                    {
                        new Chapter(),
                        new Chapter(),
                    }
                });
                RF.Save(new Book
                {
                    Name = "2.2",
                    ChapterList =
                    {
                        new Chapter(),
                        new Chapter(),
                    }
                });

                var list = repo.LinqGetByNameStringAction(StringAction.RefContains, "1");
                Assert.IsTrue(list.Count == 4);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_StringLengthNotsupported()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                try
                {
                    var list = repo.LinqGetByNameStringAction(StringAction.LengthNotSupport);
                    Assert.IsTrue(false, "Length 属性不能使用。");
                }
                catch (NotSupportedException) { }
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_ValueInArray()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book { Name = "1" });
                RF.Save(new Book { Name = "2" });
                RF.Save(new Book { Name = "3" });
                RF.Save(new Book { Name = "4" });
                var list = repo.LinqGetByBookNameInArray(new string[] { "1", "3" });
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_ValueInArray_Ref()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "1",
                    ChapterList =
                    {
                        new Chapter { Name = "1.1"},
                    }
                });
                RF.Save(new Book
                {
                    Name = "2",
                    ChapterList =
                    {
                        new Chapter { Name = "2.1"},
                        new Chapter { Name = "2.2"},
                    }
                });
                RF.Save(new Book
                {
                    Name = "3",
                    ChapterList =
                    {
                        new Chapter { Name = "3.1"},
                    }
                });

                var list = repo.LinqGetByBookNameIn(new string[] { "1", "3" });
                Assert.IsTrue(list.Count == 2);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_ValueIn_List()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book { Name = "1" });
                RF.Save(new Book { Name = "2" });
                RF.Save(new Book { Name = "3" });
                RF.Save(new Book { Name = "4" });
                var list = repo.LinqGetByBookNameInList(new List<string> { "1", "3" });
                Assert.IsTrue(list.Count == 2);
            }
        }

        /// <summary>
        /// 使用 And 连接两个分别使用 Or 连接的条件组合。
        /// </summary>
        [TestMethod]
        public void ORM_LinqQuery_BracketOrAndOr()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Code = "c1", Name = "n1" });
                repo.Save(new Book { Code = "c2", Name = "n2" });
                repo.Save(new Book { Code = "c3", Name = "n3" });
                repo.Save(new Book { Code = "c4", Name = "n4" });
                repo.Save(new Book { Code = "c5", Name = "n5" });
                repo.Save(new Book { Code = "c6", Name = "n6" });

                var list = repo.LinqGet_BracketOrAndOr("c1", "c2", "c3", "n3", "n4", "n5");
                Assert.IsTrue(list.Count == 1);
            }
        }

        /// <summary>
        /// Linq 混合 IPropertyQuery 使用。
        /// </summary>
        [TestMethod]
        public void ORM_LinqQuery_WithPropertyQuery()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Code = "c1", Name = "n1" });
                repo.Save(new Book { Code = "c2", Name = "n2" });
                repo.Save(new Book { Code = "c3", Name = "n3" });
                repo.Save(new Book { Code = "c4", Name = "n4" });
                repo.Save(new Book { Code = "c5", Name = "n5" });
                repo.Save(new Book { Code = "c6", Name = "n6" });

                var list = repo.LinqGet_WithPropertyQuery("c2", "c3", "c4", "n3", "n4", "n5");
                Assert.IsTrue(list.Count == 1);
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_TwoPropertiesConstraint()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Code = "1", Name = "2" });
                repo.Save(new Book { Code = "2", Name = "2" });
                repo.Save(new Book { Code = "3", Name = "1" });
                repo.Save(new Book { Code = "4", Name = "4" });

                var list = repo.LinqGet_NameEqualsCode();
                Assert.IsTrue(list.Count == 2);
            }
        }

        /// <summary>
        /// 一般属性与引用属性间的对比
        /// </summary>
        [TestMethod]
        public void ORM_LinqQuery_TwoPropertiesConstraint_NormalToRef()
        {
            var bcRepo = RF.Concrete<BookCategoryRepository>();
            var repo = RF.Concrete<BookRepository>();
            using (var tran = RF.TransactionScope(repo))
            {
                var bc = new BookCategory { Code = "1" };
                bcRepo.Save(bc);
                repo.Save(new Book { Code = "1", BookCategory = bc });
                repo.Save(new Book { Code = "2", BookCategory = bc });
                repo.Save(new Book { BookCategory = bc });
                repo.Save(new Book());

                var list = repo.LinqGet_BCIdEqualsRefBCId();
                Assert.IsTrue(list.Count == 3, "查询出第1、2、3个对象。");
            }
        }

        /// <summary>
        /// 单元测试：两个属性都是 RefEntity 的对比
        /// </summary>
        [TestMethod]
        public void ORM_LinqQuery_TwoPropertiesConstraint_RefToRef()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc1 = new BookCategory { Code = "1", Name = "1" };
                var bc2 = new BookCategory { Code = "2", Name = "1" };
                var bc3 = new BookCategory { Code = "3", Name = "3" };
                RF.Save(bc1);
                RF.Save(bc2);
                RF.Save(bc3);

                var bl1 = new BookLoc { Code = "1" };
                var bl2 = new BookLoc { Code = "2" };
                var bl3 = new BookLoc { Code = "3" };
                RF.Save(bl1);
                RF.Save(bl2);
                RF.Save(bl3);

                repo.Save(new Book { Name = "1", BookCategory = bc1, BookLoc = bl1 });
                repo.Save(new Book { Name = "2", BookCategory = bc2, BookLoc = bl1 });
                repo.Save(new Book { Name = "3", BookCategory = bc2, BookLoc = bl2 });
                repo.Save(new Book { Name = "4", BookCategory = bc3, BookLoc = bl1 });
                repo.Save(new Book { Name = "5", BookCategory = bc3, BookLoc = bl2 });
                repo.Save(new Book { Name = "6", BookCategory = bc3, BookLoc = bl3 });

                var list = repo.LinqGet_RefBCEqualsRefBC();
                Assert.IsTrue(list.Count == 2);
                Assert.IsTrue(list[0].Name == "1");
                Assert.IsTrue(list[1].Name == "6");
            }
        }

        [TestMethod]
        public void ORM_LinqQuery_Argument_Nullable()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { });
                repo.Save(new Book { Price = 1 });
                repo.Save(new Book { });
                repo.Save(new Book { Price = 2 });

                var list = repo.LinqGetByNullable(1);
                Assert.IsTrue(list.Count == 1);

                list = repo.LinqGetByNullable(null);
                Assert.IsTrue(list.Count == 4);
            }
        }

        //[TestMethod]
        //public void ORM_LinqQuery_Argument_Nullable_FetchBy()
        //{
        //    var repo = RF.Concrete<BookRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        repo.Save(new Book { });
        //        repo.Save(new Book { Price = 1 });
        //        repo.Save(new Book { });
        //        repo.Save(new Book { Price = 2 });

        //        var list = repo.LinqGetByNullableFetchBy(1);
        //        Assert.IsTrue(list.Count == 1);

        //        try
        //        {
        //            list = repo.LinqGetByNullableFetchBy(null);
        //            Assert.IsTrue(false, "由于有两个同时可接受 null 参数的数据层同名方法，所以需要抛出异常。");
        //        }
        //        catch (InvalidProgramException) { }
        //    }
        //}

        [TestMethod]
        public void ORM_LinqQuery_InnerJoin_TwoToSingleTable()
        {
            var repo = RF.Concrete<ArticleRepository>();
            using (RF.TransactionScope(repo))
            {
                var huqf = new BlogUser { UserName = "huqf" };
                var admin = new BlogUser { UserName = "admin" };
                var admin2 = new BlogUser { UserName = "admin2" };
                RF.Save(huqf);
                RF.Save(admin);
                RF.Save(admin2);

                repo.Save(new Article { User = huqf });
                repo.Save(new Article { User = huqf, Administrator = admin });
                repo.Save(new Article { User = huqf, Administrator = admin2 });
                repo.Save(new Article { User = admin });
                repo.Save(new Article { User = admin, Administrator = admin });
                repo.Save(new Article { User = admin, Administrator = admin2 });

                var list = repo.GetForTwoJoinTest();
                Assert.IsTrue(list.Count == 2);
                Assert.IsTrue(list[0].AdministratorId == null);
                Assert.IsTrue(list[1].AdministratorId == admin.Id);
            }
        }

        [TestMethod]
        public void ___ORM_LinqQuery_WhereChildrenExists()
        {
            //var repo = RF.Concrete<BookRepository>();
            //using (RF.TransactionScope(repo))
            //{
            //    repo.Save(new Book { });
            //    repo.Save(new Book { });
            //    repo.Save(new Book
            //    {
            //        Name = "1",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "1.1"},
            //        }
            //    });

            //    var list = repo.LinqGetIfChildrenExists();
            //    Assert.IsTrue(list.Count == 1);
            //    var book = list[0];
            //    Assert.IsTrue(book.Name == "1");
            //}
        }

        [TestMethod]
        public void ___ORM_LinqQuery_WhereChildrenExistsChapterName()
        {
            //var repo = RF.Concrete<BookRepository>();
            //using (RF.TransactionScope(repo))
            //{
            //    repo.Save(new Book { });
            //    repo.Save(new Book
            //    {
            //        Name = "1",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "1.1"},
            //        }
            //    });
            //    repo.Save(new Book
            //    {
            //        Name = "2",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "1.1"},
            //            new Chapter { Name = "2.1"},
            //        }
            //    });
            //    repo.Save(new Book
            //    {
            //        Name = "3",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "2.1"},
            //            new Chapter { Name = "2.2"},
            //        }
            //    });

            //    var list = repo.LinqGetIfChildrenExists("1.1");
            //    Assert.IsTrue(list.Count == 2);
            //}
        }

        [TestMethod]
        public void ___ORM_LinqQuery_WhereChildrenAllChapterName()
        {
            //var repo = RF.Concrete<BookRepository>();
            //using (RF.TransactionScope(repo))
            //{
            //    repo.Save(new Book { });
            //    repo.Save(new Book
            //    {
            //        Name = "1",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "1.1"},
            //        }
            //    });
            //    repo.Save(new Book
            //    {
            //        Name = "2",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "1.1"},
            //            new Chapter { Name = "1.1"},
            //        }
            //    });
            //    repo.Save(new Book
            //    {
            //        Name = "3",
            //        ChapterList =
            //        {
            //            new Chapter { Name = "2.1"},
            //            new Chapter { Name = "2.2"},
            //        }
            //    });

            //    var list = repo.LinqGetIfChildrenAll("1.1");
            //    Assert.IsTrue(list.Count == 1);
            //    Assert.IsTrue(list[1].Name == "1");
            //    Assert.IsTrue(list[2].Name == "2");
            //}
        }

        #endregion

        #region AggtSQL

        [TestMethod]
        public void ORM_AggtSQL_LoadReferenceEntities()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var so = new SectionOwner();
                RF.Save(so);

                var book = CreateAggtBook(so);
                RF.Save(book);

                var api = AggregateSQL.Instance;

                var loadOptions = api
                    .BeginLoadOptions<Book>()
                    .LoadChildren(pp => pp.ChapterList)
                    .Continue<Chapter>().LoadChildren(c => c.SectionList)
                    .Order<Section>().By(v => v.SectionOwner.Name)
                    .LoadFK(v => v.SectionOwner);

                var sql = api.GenerateQuerySQL(loadOptions, book.Id);

                //聚合加载整个对象树。
                var entities = new BookList();
                api.LoadEntities(entities, sql, loadOptions);

                //无懒加载测试。
                var count = Logger.DbAccessedCount;
                foreach (Book book2 in entities)
                {
                    foreach (Chapter chapter in book2.ChapterList)
                    {
                        foreach (Section section in chapter.SectionList)
                        {
                            var so2 = section.SectionOwner;
                        }
                    }
                }
                Assert.IsTrue(Logger.DbAccessedCount == count, "由于数据已经全部加载完成，所以这里不会发生懒加载。");
            }
        }

        [TestMethod]
        public void ORM_AggtSQL_LoadEntities()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var so = new SectionOwner();
                RF.Save(so);

                var book = CreateAggtBook(so);
                RF.Save(book);

                var api = AggregateSQL.Instance;

                var loadOptions = api
                    .BeginLoadOptions<Chapter>().LoadChildren(c => c.SectionList)
                    .Order<Section>().By(p => p.Name)
                    //.Continue<PBSProperty>()
                    .LoadFK(p => p.SectionOwner);

                var sql = api.GenerateQuerySQL(loadOptions, book.Id);
                var sql2 = api.GenerateQuerySQL(loadOptions, string.Format("Chapter.BookId = {0}", book.Id));

                Assert.AreEqual(sql, sql2);

                //聚合加载整个对象树。
                var entities = new ChapterList();
                api.LoadEntities(entities, sql, loadOptions);

                //无懒加载测试。
                var count = Logger.DbAccessedCount;
                foreach (Chapter chapter in entities)
                {
                    foreach (Section section in chapter.SectionList)
                    {
                        var so2 = section.SectionOwner;
                    }
                }
                Assert.IsTrue(Logger.DbAccessedCount == count, "由于数据已经全部加载完成，所以这里不会发生懒加载。");
            }
        }

        [TestMethod]
        public void ORM_AggtSQL_LoadSingleChild()
        {
            var bookId = 0;
            var sqlSimple = AggregateSQL.Instance.GenerateQuerySQL<Chapter>(
                option => option.LoadChildren(c => c.SectionList),
                bookId
                );
            var list = new ChapterList();
            AggregateSQL.Instance.LoadEntities<Chapter>(list,
                option => option.LoadChildren(c => c.SectionList),
                bookId
                );
        }

        [TestMethod]
        public void ORM_AggtSQL_JoinWhere()
        {
            var sqlSimple = AggregateSQL.Instance.GenerateQuerySQL<Section>(
                option => option.LoadFK(s => s.SectionOwner),
                "Section.Name = 'testing'",
                "JOIN Chapter on Chapter.Id = Section.ChapterId"
                );
        }

        private static Book CreateAggtBook(SectionOwner so)
        {
            var book = new Book
            {
                ChapterList =
                {
                    new Chapter
                    {
                        SectionList = 
                        {
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                        }
                    },
                    new Chapter
                    {
                        SectionList = 
                        {
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                        }
                    },
                    new Chapter
                    {
                        SectionList = 
                        {
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                            new Section{ SectionOwner = so },
                        }
                    }
                }
            };
            return book;
        }

        #endregion

        #region 分页查询

        /*********************** 代码块解释 *********************************
         * 
         * 以下测试：如果 TestUser 类是映射到 SQLCe 数据库，则是测试内存分页，否则则是在测试数据库分页。
         * 
        **********************************************************************/

        [TestMethod]
        public void ORM_PagingQuery_GetAll()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                for (int i = 0; i < 10; i++)
                {
                    var user = new TestUser();
                    repo.Save(user);
                }

                //第二页，每页两条。
                var pagingInfo = new PagingInfo(2, 2, true);
                var items = repo.GetAll(pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == 10, "总条数必须等于 10 条。");
                Assert.IsTrue(items.TotalCount == 10, "总条数必须等于 10 条。");

                //第 6 页，每页两条。
                pagingInfo = new PagingInfo(6, 2, true);
                items = repo.GetAll(pagingInfo);
                Assert.IsTrue(items.Count == 0, "应该没有第六页的数据。");
                Assert.IsTrue(pagingInfo.TotalCount == 10, "总条数必须等于 10 条。");
                Assert.IsTrue(items.TotalCount == 10, "总条数必须等于 10 条。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_GetAll_Top()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                for (int i = 0; i < 10; i++)
                {
                    var user = new TestUser();
                    repo.Save(user);
                }

                //第一页，每页两条。（TOP）
                var pagingInfo = new PagingInfo(1, 2, false);
                var items = repo.GetAll(pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");

                pagingInfo = new PagingInfo(1, 2, true);
                items = repo.GetAll(pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == 10, "总条数必须等于 10 条。");
                Assert.IsTrue(items.TotalCount == 10, "总条数必须等于 10 条。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_GetByParentId()
        {
            var taskRepo = RF.Concrete<TestTreeTaskRepository>();
            var userRepo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(userRepo))
            {
                var user = new TestUser
                {
                    TestTreeTaskList =
                    {
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                    }
                };
                userRepo.Save(user);

                //第二页，每页两条。
                var pagingInfo = new PagingInfo(2, 2, true);
                var items = taskRepo.GetByParentId(user.Id, pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == user.TestTreeTaskList.Count, "pagingInfo.TotalCount 总条数不一致。");
                Assert.IsTrue(items.TotalCount == user.TestTreeTaskList.Count, "EntityList.TotalCount 总条数不一致。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_GetByParentId_Top()
        {
            var taskRepo = RF.Concrete<TestTreeTaskRepository>();
            var userRepo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(userRepo))
            {
                var user = new TestUser
                {
                    TestTreeTaskList =
                    {
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                        new TestTreeTask(),
                    }
                };
                userRepo.Save(user);

                //第一页，每页两条。（TOP）
                var pagingInfo = new PagingInfo(1, 2, false);
                var items = taskRepo.GetByParentId(user.Id, pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");

                pagingInfo = new PagingInfo(1, 2, true);
                items = taskRepo.GetByParentId(user.Id, pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == user.TestTreeTaskList.Count, "pagingInfo.TotalCount 总条数不一致。");
                Assert.IsTrue(items.TotalCount == user.TestTreeTaskList.Count, "EntityList.TotalCount 总条数不一致。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_ByRawSql()
        {
            var roleRepo = RF.Concrete<TestRoleRepository>();
            var userRepo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(userRepo))
            {
                var user = new TestUser();
                userRepo.Save(user);
                var rowsCount = 10;
                for (int i = 0; i < rowsCount; i++)
                {
                    roleRepo.Save(new TestRole { TestUser = user });
                }

                //第二页，每页两条。
                var pagingInfo = new PagingInfo(2, 2, true);
                var items = roleRepo.GetByRawSql(
@"Select Roles.*
FROM Roles 
    INNER JOIN USERS On Roles.UserId = Users.Id
WHERE Roles.Id > {0} 
ORDER BY Roles.Id DESC, Roles.name DESC", new object[] { 0 }, pagingInfo);

                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == rowsCount, "pagingInfo.TotalCount 总条数不一致。");
                Assert.IsTrue(items.TotalCount == rowsCount, "EntityList.TotalCount 总条数不一致。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_ByRawSql_Top()
        {
            var roleRepo = RF.Concrete<TestRoleRepository>();
            var userRepo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(userRepo))
            {
                var user = new TestUser();
                userRepo.Save(user);
                var rowsCount = 10;
                for (int i = 0; i < rowsCount; i++)
                {
                    roleRepo.Save(new TestRole { TestUser = user });
                }

                var sql = @"Select Roles.*
FROM Roles 
    INNER JOIN USERS On Roles.UserId = Users.Id
WHERE Roles.Id > {0} 
ORDER BY Roles.Id DESC, Roles.name DESC";
                var parameters = new object[] { 0 };

                //第一页，每页两条。（TOP）
                var pagingInfo = new PagingInfo(1, 2, false);
                var items = roleRepo.GetByRawSql(sql, parameters, pagingInfo);

                pagingInfo = new PagingInfo(1, 2, true);
                items = roleRepo.GetByRawSql(sql, parameters, pagingInfo);
                Assert.IsTrue(items.Count == 2, "分页查询的结果应该只有两条。");
                Assert.IsTrue(pagingInfo.TotalCount == rowsCount, "pagingInfo.TotalCount 总条数不一致。");
                Assert.IsTrue(items.TotalCount == rowsCount, "EntityList.TotalCount 总条数不一致。");
            }
        }

        [TestMethod]
        public void ORM_PagingQuery_ByMultiParameters()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new TestUser { Age = 1, Name = "user" });
                repo.Save(new TestUser { Age = 1, Name = "user" });
                repo.Save(new TestUser { Age = 1, Name = "user" });

                var pagingInfo = new PagingInfo(2, 1, true);
                var list = repo.GetByNameAgeByMultiParameters("user", 1, pagingInfo);
                Assert.IsTrue(list.Count == 1);
                Assert.IsTrue(pagingInfo.TotalCount == 3);

                list = repo.GetByNameAgeByMultiParameters("user", 1, PagingInfo.Empty);
                Assert.IsTrue(list.Count == 3);

                list = repo.GetByNameAgeByMultiParameters("user", 1, null);
                Assert.IsTrue(list.Count == 3);
            }
        }

        #endregion

        #region SqlTree

        /// <summary>
        /// 生成可用于 SqlServer 数据库的一个简单的条件查询语句。
        /// </summary>
        [TestMethod]
        public void ORM_SqlTree_Select_SqlServer()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };

            var generator = new SqlServerSqlGenerator();
            generator.Generate(select);
            var sql = generator.Sql;

            Assert.IsTrue(sql.ToString() ==
@"SELECT [Table1].[Column1], [Table1].[Column2]
FROM [Table1]
WHERE [Table1].[Column2] = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        /// <summary>
        /// 生成可用于 Oracle 数据库的一个简单的条件查询语句。
        /// </summary>
        [TestMethod]
        public void ORM_SqlTree_Select_Oracle()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };

            var generator = new OracleSqlGenerator();
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT ""TABLE1"".""COLUMN1"", ""TABLE1"".""COLUMN2""
FROM ""TABLE1""
WHERE ""TABLE1"".""COLUMN2"" = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Simplest()
        {
            var select = new SqlSelect();
            select.Selection = new SqlSelectAll();
            select.From = new SqlTable { TableName = "Table1" };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Table1");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Star()
        {
            var select = new SqlSelect();
            select.Selection = new SqlSelectAll();
            select.From = new SqlTable { TableName = "Table1" };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Table1");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Top()
        {
            var select = new SqlSelect();
            select.Top = 10;
            select.Selection = new SqlSelectAll();
            select.From = new SqlTable { TableName = "Table1" };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT TOP 10 *
FROM Table1");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Distinct()
        {
            var select = new SqlSelect();
            select.IsDistinct = true;
            select.Selection = new SqlSelectAll();
            select.From = new SqlTable { TableName = "Table1" };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            Assert.IsTrue(generator.Sql.ToString() == @"SELECT DISTINCT *
FROM Table1");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Count()
        {
            var select = new SqlSelect();
            select.IsCounting = true;
            select.From = new SqlTable { TableName = "Table1" };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            Assert.IsTrue(generator.Sql.ToString() == @"SELECT COUNT(0)
FROM Table1");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_WithoutQuota()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT Table1.Column1, Table1.Column2
FROM Table1
WHERE Table1.Column2 = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Where_AndOr()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1" };
            select.Selection = new SqlSelectAll();
            select.From = table;
            select.Where = new SqlBinaryConstraint
            {
                Left = new SqlColumnConstraint
                {
                    Column = new SqlColumn { Table = table, ColumnName = "Column1" },
                    Operator = SqlColumnConstraintOperator.Equal,
                    Value = "A"
                },
                Opeartor = SqlBinaryConstraintType.Or,
                Right = new SqlBinaryConstraint
                {
                    Left = new SqlBinaryConstraint
                    {
                        Left = new SqlColumnConstraint
                        {
                            Column = new SqlColumn { Table = table, ColumnName = "Column2" },
                            Operator = SqlColumnConstraintOperator.Equal,
                            Value = "A2"
                        },
                        Opeartor = SqlBinaryConstraintType.Or,
                        Right = new SqlColumnConstraint
                        {
                            Column = new SqlColumn { Table = table, ColumnName = "Column2" },
                            Operator = SqlColumnConstraintOperator.Equal,
                            Value = "B2"
                        }
                    },
                    Opeartor = SqlBinaryConstraintType.And,
                    Right = new SqlColumnConstraint
                    {
                        Column = new SqlColumn { Table = table, ColumnName = "Column1" },
                        Operator = SqlColumnConstraintOperator.Equal,
                        Value = "B"
                    }
                }
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Table1
WHERE Table1.Column1 = {0} OR (Table1.Column2 = {1} OR Table1.Column2 = {2}) AND Table1.Column1 = {3}");
            Assert.IsTrue(sql.Parameters.Count == 4);
            Assert.IsTrue(sql.Parameters[0].ToString() == "A");
            Assert.IsTrue(sql.Parameters[1].ToString() == "A2");
            Assert.IsTrue(sql.Parameters[2].ToString() == "B2");
            Assert.IsTrue(sql.Parameters[3].ToString() == "B");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Alias()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1", Alias = "t1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1", Alias = "c1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2", Alias = "c2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Column1 AS c1, t1.Column2 AS c2
FROM Table1 AS t1
WHERE t1.Column2 = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_OrderBy()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1", Alias = "t1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1", Alias = "c1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2", Alias = "c2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };
            select.OrderBy.Add(new SqlOrderBy
            {
                Column = column2,
                Direction = OrderDirection.Descending
            });

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Column1 AS c1, t1.Column2 AS c2
FROM Table1 AS t1
WHERE t1.Column2 = {0}
ORDER BY t1.Column2 DESC");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_OrderBy2()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Table1", Alias = "t1" };
            var column1 = new SqlColumn { Table = table, ColumnName = "Column1", Alias = "c1" };
            var column2 = new SqlColumn { Table = table, ColumnName = "Column2", Alias = "c2" };
            select.Selection = new SqlArray
            {
                Items = { column1, column2 }
            };
            select.From = table;
            select.Where = new SqlColumnConstraint
            {
                Column = column2,
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "Column2Value"
            };
            select.OrderBy.Add(new SqlOrderBy
            {
                Column = column1,
                Direction = OrderDirection.Ascending
            });
            select.OrderBy.Add(new SqlOrderBy
            {
                Column = column2,
                Direction = OrderDirection.Descending
            });

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Column1 AS c1, t1.Column2 AS c2
FROM Table1 AS t1
WHERE t1.Column2 = {0}
ORDER BY t1.Column1 ASC, t1.Column2 DESC");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "Column2Value");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_InnerJoin()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Article", Alias = "a" };
            var userTable = new SqlTable { TableName = "User", Alias = "u" };
            select.Selection = new SqlArray
            {
                Items =
                {
                    new SqlSelectAll{ Table = table },
                    new SqlSelectAll{ Table = userTable },
                }
            };
            select.From = new SqlJoin
            {
                Left = table,
                JoinType = SqlJoinType.Inner,
                Right = userTable,
                Condition = new SqlColumnsComparisonConstraint
                {
                    LeftColumn = new SqlColumn { Table = table, ColumnName = "UserId" },
                    RightColumn = new SqlColumn { Table = userTable, ColumnName = "Id" },
                }
            };
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = userTable, ColumnName = "UserName" },
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "HuQingfang"
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*, u.*
FROM Article AS a
    INNER JOIN User AS u ON a.UserId = u.Id
WHERE u.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_InnerJoin_TwoToSingleTable()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Article", Alias = "a" };
            var userTable1 = new SqlTable { TableName = "User", Alias = "u1" };
            var userTable2 = new SqlTable { TableName = "User", Alias = "u2" };
            select.Selection = new SqlSelectAll { Table = table };
            select.From = new SqlJoin
            {
                Left = new SqlJoin
                {
                    Left = table,
                    JoinType = SqlJoinType.Inner,
                    Right = userTable1,
                    Condition = new SqlColumnsComparisonConstraint
                    {
                        LeftColumn = new SqlColumn { Table = table, ColumnName = "UserId" },
                        RightColumn = new SqlColumn { Table = userTable1, ColumnName = "Id" },
                    }
                },
                JoinType = SqlJoinType.Inner,
                Right = userTable2,
                Condition = new SqlColumnsComparisonConstraint
                {
                    LeftColumn = new SqlColumn { Table = table, ColumnName = "AdministratorId" },
                    RightColumn = new SqlColumn { Table = userTable2, ColumnName = "Id" },
                }
            };
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = userTable2, ColumnName = "UserName" },
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "HuQingfang"
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*
FROM Article AS a
    INNER JOIN User AS u1 ON a.UserId = u1.Id
    INNER JOIN User AS u2 ON a.AdministratorId = u2.Id
WHERE u2.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_OuterJoin()
        {
            var select = new SqlSelect();
            var table = new SqlTable { TableName = "Article", Alias = "a" };
            var userTable = new SqlTable { TableName = "User", Alias = "u" };
            select.Selection = new SqlSelectAll { Table = table };
            select.From = new SqlJoin
            {
                Left = table,
                JoinType = SqlJoinType.LeftOuter,
                Right = userTable,
                Condition = new SqlColumnsComparisonConstraint
                {
                    LeftColumn = new SqlColumn { Table = table, ColumnName = "UserId" },
                    RightColumn = new SqlColumn { Table = userTable, ColumnName = "Id" },
                }
            };
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = userTable, ColumnName = "UserName" },
                Operator = SqlColumnConstraintOperator.Equal,
                Value = "HuQingfang"
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*
FROM Article AS a
    LEFT OUTER JOIN User AS u ON a.UserId = u.Id
WHERE u.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_InSubSelect()
        {
            var select = new SqlSelect();
            var articleTable = new SqlTable { TableName = "Article" };
            var subSelect = new SqlSelect
            {
                Selection = new SqlColumn { Table = articleTable, ColumnName = "UserId" },
                From = articleTable,
                Where = new SqlColumnConstraint
                {
                    Column = new SqlColumn { Table = articleTable, ColumnName = "CreateDate" },
                    Operator = SqlColumnConstraintOperator.Equal,
                    Value = DateTime.Today
                }
            };

            var userTable = new SqlTable { TableName = "User" };
            select.Selection = new SqlSelectAll();
            select.From = userTable;
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = userTable, ColumnName = "Id" },
                Operator = SqlColumnConstraintOperator.In,
                Value = subSelect
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM User
WHERE User.Id IN (
    SELECT Article.UserId
    FROM Article
    WHERE Article.CreateDate = {0}
)");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].Equals(DateTime.Today));
        }

        [TestMethod]
        public void ORM_SqlTree_Select_InSubSelect_Join()
        {
            var select = new SqlSelect();
            var adminTable = new SqlTable { TableName = "User", Alias = "Administrator" };
            var articleTable = new SqlTable { TableName = "Article" };
            var subSelect = new SqlSelect
            {
                Selection = new SqlColumn { Table = articleTable, ColumnName = "UserId" },
                From = new SqlJoin
                {
                    Left = articleTable,
                    JoinType = SqlJoinType.Inner,
                    Right = adminTable,
                    Condition = new SqlColumnsComparisonConstraint
                    {
                        LeftColumn = new SqlColumn { Table = articleTable, ColumnName = "AdministratorId" },
                        RightColumn = new SqlColumn { Table = adminTable, ColumnName = "Id" },
                    }
                },
                Where = new SqlColumnConstraint
                {
                    Column = new SqlColumn { Table = adminTable, ColumnName = "Id" },
                    Operator = SqlColumnConstraintOperator.Equal,
                    Value = 1
                }
            };

            var userTable = new SqlTable { TableName = "User" };
            select.Selection = new SqlSelectAll();
            select.From = userTable;
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = userTable, ColumnName = "Id" },
                Operator = SqlColumnConstraintOperator.In,
                Value = subSelect
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM User
WHERE User.Id IN (
    SELECT Article.UserId
    FROM Article
        INNER JOIN User AS Administrator ON Article.AdministratorId = Administrator.Id
    WHERE Administrator.Id = {0}
)");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].Equals(1));
        }

        [TestMethod]
        public void ORM_SqlTree_Select_ChildrenExists()
        {
            var articleTable = new SqlTable { TableName = "Article" };
            var userTable = new SqlTable { TableName = "User", Alias = "u" };
            var select = new SqlSelect();
            select.Selection = new SqlSelectAll();
            select.From = userTable;
            select.Where = new SqlBinaryConstraint
            {
                Left = new SqlColumnConstraint
                {
                    Column = new SqlColumn { Table = userTable, ColumnName = "Id" },
                    Operator = SqlColumnConstraintOperator.Greater,
                    Value = 0
                },
                Opeartor = SqlBinaryConstraintType.And,
                Right = new SqlExistsConstraint
                {
                    Select = new SqlSelect
                    {
                        Selection = new SqlLiteral { FormattedSql = "0" },
                        From = articleTable,
                        Where = new SqlColumnsComparisonConstraint
                        {
                            LeftColumn = new SqlColumn { Table = articleTable, ColumnName = "UserId" },
                            RightColumn = new SqlColumn { Table = userTable, ColumnName = "Id" }
                        }
                    }
                }
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM User AS u
WHERE u.Id > {0} AND EXISTS (
    SELECT 0
    FROM Article
    WHERE Article.UserId = u.Id
)");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_ChildrenAll()
        {
            var articleTable = new SqlTable { TableName = "Article" };
            var userTable = new SqlTable { TableName = "User", Alias = "u" };
            var select = new SqlSelect
            {
                Selection = new SqlSelectAll(),
                From = userTable,
                Where = new SqlBinaryConstraint
                {
                    Left = new SqlColumnConstraint
                    {
                        Column = new SqlColumn { Table = userTable, ColumnName = "Id" },
                        Operator = SqlColumnConstraintOperator.Greater,
                        Value = 0
                    },
                    Opeartor = SqlBinaryConstraintType.And,
                    Right = new SqlNotConstraint
                    {
                        Constraint = new SqlExistsConstraint
                        {
                            Select = new SqlSelect
                            {
                                Selection = new SqlLiteral { FormattedSql = "0" },
                                From = articleTable,
                                Where = new SqlBinaryConstraint
                                {
                                    Left = new SqlColumnsComparisonConstraint
                                    {
                                        LeftColumn = new SqlColumn { Table = articleTable, ColumnName = "UserId" },
                                        RightColumn = new SqlColumn { Table = userTable, ColumnName = "Id" }
                                    },
                                    Opeartor = SqlBinaryConstraintType.And,
                                    Right = new SqlColumnConstraint
                                    {
                                        Column = new SqlColumn { Table = articleTable, ColumnName = "Id" },
                                        Operator = SqlColumnConstraintOperator.Greater,
                                        Value = 0
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM User AS u
WHERE u.Id > {0} AND NOT (EXISTS (
    SELECT 0
    FROM Article
    WHERE Article.UserId = u.Id AND Article.Id > {1}
))");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_SelectFromSelectResult()
        {
            var userTable = new SqlTable { TableName = "User" };
            var subSelectRef = new SqlSubSelect
            {
                Select = new SqlSelect
                {
                    Selection = new SqlSelectAll(),
                    From = userTable,
                    Where = new SqlColumnConstraint
                    {
                        Column = new SqlColumn { Table = userTable, ColumnName = "Id" },
                        Operator = SqlColumnConstraintOperator.Greater,
                        Value = 0
                    }
                },
                Alias = "T"
            };

            var select = new SqlSelect();
            select.Selection = new SqlSelectAll();
            select.From = subSelectRef;
            select.Where = new SqlColumnConstraint
            {
                Column = new SqlColumn { Table = subSelectRef, ColumnName = "Id" },
                Operator = SqlColumnConstraintOperator.Less,
                Value = 100
            };

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;

            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM (
    SELECT *
    FROM User
    WHERE User.Id > {0}
) AS T
WHERE T.Id < {1}");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Paging()
        {
            var table = new SqlTable { TableName = "ASN" };
            var pk = new SqlColumn { Table = table, ColumnName = "Id" };

            var select = new SqlSelect
            {
                Selection = new SqlSelectAll(),
                From = table,
                Where = new SqlColumnConstraint
                {
                    Column = pk,
                    Operator = SqlColumnConstraintOperator.Greater,
                    Value = 0
                },
                OrderBy = new List<SqlOrderBy>
                {
                    new SqlOrderBy
                    { 
                        Column = new SqlColumn{ Table = table, ColumnName = "AsnCode" },
                        Direction = OrderDirection.Ascending
                    }
                }
            };
            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM ASN
WHERE ASN.Id > {0}
ORDER BY ASN.AsnCode ASC");

            var pagedSelect = generator.ModifyToPagingTree(select, pk, new PagingInfo(3, 10));

            generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(pagedSelect);
            var pagingSql = generator.Sql;
            Assert.IsTrue(pagingSql.ToString() ==
@"SELECT TOP 10 *
FROM ASN
WHERE ASN.Id > {0} AND ASN.Id NOT IN (
    SELECT TOP 20 ASN.Id
    FROM ASN
    WHERE ASN.Id > {1}
    ORDER BY ASN.AsnCode ASC
)
ORDER BY ASN.AsnCode ASC");
        }

        [TestMethod]
        public void ORM_SqlTree_Select_Paging_PageNumer1()
        {
            var table = new SqlTable { TableName = "ASN" };
            var pk = new SqlColumn { Table = table, ColumnName = "Id" };

            var select = new SqlSelect
            {
                Selection = new SqlSelectAll(),
                From = table,
                Where = new SqlColumnConstraint
                {
                    Column = pk,
                    Operator = SqlColumnConstraintOperator.Greater,
                    Value = 0
                },
                OrderBy = new List<SqlOrderBy>
                {
                    new SqlOrderBy
                    { 
                        Column = new SqlColumn{ Table = table, ColumnName = "AsnCode" },
                        Direction = OrderDirection.Ascending
                    }
                }
            };
            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(select);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM ASN
WHERE ASN.Id > {0}
ORDER BY ASN.AsnCode ASC");

            var pagedSelect = generator.ModifyToPagingTree(select, pk, new PagingInfo(1, 10));

            generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(pagedSelect);
            var pagingSql = generator.Sql;
            Assert.IsTrue(pagingSql.ToString() ==
@"SELECT TOP 10 *
FROM ASN
WHERE ASN.Id > {0}
ORDER BY ASN.AsnCode ASC");
        }

        #endregion

        #region TableQuery

        [TestMethod]
        public void ORM_TableQuery_ForDemo()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(code, PropertyOperator.Equal, "code")
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT Book.Code, Book.Name
FROM Book
WHERE Book.Code = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "code");
        }

        [TestMethod]
        public void ORM_TableQuery_Where_AndOr_Demo()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                from: table,
                where: f.Or(
                    left: f.Constraint(code, "A"),
                    right: f.And(
                        left: f.Or(
                            left: f.Constraint(name, "A2"),
                            right: f.Constraint(name, "B2")
                        ),
                        right: f.Constraint(code, "B")
                    )
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM Book
WHERE Book.Code = {0} OR (Book.Name = {1} OR Book.Name = {2}) AND Book.Code = {3}");
            Assert.IsTrue(sql.Parameters.Count == 4);
            Assert.IsTrue(sql.Parameters[0].ToString() == "A");
            Assert.IsTrue(sql.Parameters[1].ToString() == "A2");
            Assert.IsTrue(sql.Parameters[2].ToString() == "B2");
            Assert.IsTrue(sql.Parameters[3].ToString() == "B");
        }

        [TestMethod]
        public void ORM_TableQuery_SqlServer()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(column: code, value: "code", op: PropertyOperator.Equal)
            );

            var generator = new SqlServerSqlGenerator();
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT [Book].[Code], [Book].[Name]
FROM [Book]
WHERE [Book].[Code] = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "code");
        }

        [TestMethod]
        public void ORM_TableQuery_Oracle()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(column: code, value: "code", op: PropertyOperator.Equal)
            );

            var generator = new OracleSqlGenerator();
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT ""BOOK"".""CODE"", ""BOOK"".""NAME""
FROM ""BOOK""
WHERE ""BOOK"".""CODE"" = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "code");
        }

        [TestMethod]
        public void ORM_TableQuery_Simplest()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var query = f.Query(
                from: table
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Book");
        }

        [TestMethod]
        public void ORM_TableQuery_Star()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var query = f.Query(
                selection: f.SelectAll(),
                from: table
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Book");
        }

        [TestMethod]
        public void ORM_TableQuery_Top()
        {
            var f = QueryFactory.Instance;
            var query = f.Query(
                top: 10,
                from: f.Table<Book>()
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT TOP 10 *
FROM Book");
        }

        [TestMethod]
        public void ORM_TableQuery_Distinct()
        {
            var f = QueryFactory.Instance;
            var query = f.Query(
                isDistinct: true,
                from: f.Table<Book>()
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            Assert.IsTrue(generator.Sql.ToString() == @"SELECT DISTINCT *
FROM Book");
        }

        [TestMethod]
        public void ORM_TableQuery_Count()
        {
            var f = QueryFactory.Instance;
            var query = f.Query(
                isCounting: true,
                from: f.Table<Book>()
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            Assert.IsTrue(generator.Sql.ToString() == @"SELECT COUNT(0)
FROM Book");
        }

        [TestMethod]
        public void ORM_TableQuery_WithoutQuota()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(column: code, value: "code", op: PropertyOperator.Equal)
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT Book.Code, Book.Name
FROM Book
WHERE Book.Code = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "code");
        }

        [TestMethod]
        public void ORM_TableQuery_Where_AndOr()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>();
            var code = table.Column(Book.CodeProperty);
            var name = table.Column(Book.NameProperty);
            var query = f.Query(
                from: table,
                where: f.Binary(
                    left: f.Constraint(column: code, value: "A"),
                    op: BinaryOperator.Or,
                    right: f.Binary(
                        left: f.Binary(
                            left: f.Constraint(column: name, value: "A2"),
                            op: BinaryOperator.Or,
                            right: f.Constraint(column: name, value: "B2")
                        ),
                        op: BinaryOperator.And,
                        right: f.Constraint(column: code, value: "B")
                    )
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM Book
WHERE Book.Code = {0} OR (Book.Name = {1} OR Book.Name = {2}) AND Book.Code = {3}");
            Assert.IsTrue(sql.Parameters.Count == 4);
            Assert.IsTrue(sql.Parameters[0].ToString() == "A");
            Assert.IsTrue(sql.Parameters[1].ToString() == "A2");
            Assert.IsTrue(sql.Parameters[2].ToString() == "B2");
            Assert.IsTrue(sql.Parameters[3].ToString() == "B");
        }

        [TestMethod]
        public void ORM_TableQuery_Alias()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>("t1");
            var code = table.Column(Book.CodeProperty, "c1");
            var name = table.Column(Book.NameProperty, "c2");
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(code, "code")
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Code AS c1, t1.Name AS c2
FROM Book AS t1
WHERE t1.Code = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "code");
        }

        [TestMethod]
        public void ORM_TableQuery_OrderBy()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>("t1");
            var code = table.Column(Book.CodeProperty, "c1");
            var name = table.Column(Book.NameProperty, "c2");
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(name, "name"),
                orderBy: new List<IOrderBy> { f.OrderBy(name, OrderDirection.Descending) }
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Code AS c1, t1.Name AS c2
FROM Book AS t1
WHERE t1.Name = {0}
ORDER BY t1.Name DESC");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "name");
        }

        [TestMethod]
        public void ORM_TableQuery_OrderBy2()
        {
            var f = QueryFactory.Instance;
            var table = f.Table<Book>("t1");
            var code = table.Column(Book.CodeProperty, "c1");
            var name = table.Column(Book.NameProperty, "c2");
            var query = f.Query(
                selection: f.Array(code, name),
                from: table,
                where: f.Constraint(name, "name"),
                orderBy: new List<IOrderBy> { f.OrderBy(code), f.OrderBy(name, OrderDirection.Descending) }
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT t1.Code AS c1, t1.Name AS c2
FROM Book AS t1
WHERE t1.Name = {0}
ORDER BY t1.Code ASC, t1.Name DESC");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "name");
        }

        [TestMethod]
        public void ORM_TableQuery_InnerJoin()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>(), "a");
            var userSource = f.Table(RF.Concrete<BlogUserRepository>(), "u");
            var query = f.Query(
                selection: f.Array(f.SelectAll(articleSource), f.SelectAll(userSource)),
                from: f.Join(
                    left: articleSource,
                    right: userSource,
                    condition: f.Constraint(
                        leftColumn: articleSource.Column(Article.UserIdProperty),
                        rightColumn: userSource.Column(BlogUser.IdProperty)
                    )
                ),
                where: f.Constraint(userSource.Column(BlogUser.UserNameProperty), "HuQingfang")
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*, u.*
FROM Article AS a
    INNER JOIN BlogUser AS u ON a.UserId = u.Id
WHERE u.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_TableQuery_InnerJoin_TwoToSingleTable()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>(), "a");
            var userSource1 = f.Table(RF.Concrete<BlogUserRepository>(), "u1");
            var userSource2 = f.Table(RF.Concrete<BlogUserRepository>(), "u2");
            var query = f.Query(
                selection: f.SelectAll(articleSource),
                from: f.Join(
                    left: f.Join(
                        left: articleSource,
                        right: userSource1,
                        condition: f.Constraint(
                            leftColumn: articleSource.Column(Article.UserIdProperty),
                            rightColumn: userSource1.Column(BlogUser.IdProperty)
                        )
                    ),
                    right: userSource2,
                    condition: f.Constraint(
                        leftColumn: articleSource.Column(Article.AdministratorIdProperty),
                        rightColumn: userSource2.Column(BlogUser.IdProperty)
                    )
                ),
                where: f.Constraint(userSource2.Column(BlogUser.UserNameProperty), "HuQingfang")
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*
FROM Article AS a
    INNER JOIN BlogUser AS u1 ON a.UserId = u1.Id
    INNER JOIN BlogUser AS u2 ON a.AdministratorId = u2.Id
WHERE u2.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_TableQuery_OuterJoin()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>(), "a");
            var userSource = f.Table(RF.Concrete<BlogUserRepository>(), "u");
            var query = f.Query(
                selection: f.SelectAll(articleSource),
                from: f.Join(
                    left: articleSource,
                    joinType: JoinType.LeftOuter,
                    right: userSource,
                    condition: f.Constraint(
                        leftColumn: articleSource.Column(Article.UserIdProperty),
                        rightColumn: userSource.Column(BlogUser.IdProperty)
                    )
                ),
                where: f.Constraint(userSource.Column(BlogUser.UserNameProperty), "HuQingfang")
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT a.*
FROM Article AS a
    LEFT OUTER JOIN BlogUser AS u ON a.UserId = u.Id
WHERE u.UserName = {0}");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].ToString() == "HuQingfang");
        }

        [TestMethod]
        public void ORM_TableQuery_InSubSelect()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>());
            var userSource = f.Table(RF.Concrete<BlogUserRepository>());
            var query = f.Query(
                from: userSource,
                where: f.Constraint(
                    column: userSource.Column(BlogUser.IdProperty),
                    op: PropertyOperator.In,
                    value: f.Query(
                        selection: articleSource.Column(Article.UserIdProperty),
                        from: articleSource,
                        where: f.Constraint(articleSource.Column(Article.CreateDateProperty), DateTime.Today)
                    )
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;

            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM BlogUser
WHERE BlogUser.Id IN (
    SELECT Article.UserId
    FROM Article
    WHERE Article.CreateDate = {0}
)");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].Equals(DateTime.Today));
        }

        [TestMethod]
        public void ORM_TableQuery_InSubSelect_Join()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>());
            var userSource = f.Table(RF.Concrete<BlogUserRepository>());
            var adminSource = f.Table(RF.Concrete<BlogUserRepository>(), "Administrator");

            var query = f.Query(
                from: userSource,
                where: f.Constraint(
                    column: userSource.Column(BlogUser.IdProperty),
                    op: PropertyOperator.In,
                    value: f.Query(
                        selection: articleSource.Column(Article.UserIdProperty),
                        from: f.Join(
                            left: articleSource,
                            right: adminSource,
                            condition: f.Constraint(
                                articleSource.Column(Article.AdministratorIdProperty),
                                adminSource.Column(BlogUser.IdProperty)
                            )
                        ),
                        where: f.Constraint(adminSource.Column(BlogUser.IdProperty), 1)
                    )
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM BlogUser
WHERE BlogUser.Id IN (
    SELECT Article.UserId
    FROM Article
        INNER JOIN BlogUser AS Administrator ON Article.AdministratorId = Administrator.Id
    WHERE Administrator.Id = {0}
)");
            Assert.IsTrue(sql.Parameters.Count == 1);
            Assert.IsTrue(sql.Parameters[0].Equals(1));
        }

        [TestMethod]
        public void ORM_TableQuery_ChildrenExists()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>());
            var userSource = f.Table(RF.Concrete<BlogUserRepository>(), "u");

            var query = f.Query(
                from: userSource,
                where: f.And(
                    f.Constraint(
                        userSource.Column(Entity.IdProperty),
                        PropertyOperator.Greater, 0
                    ),
                    f.Exists(f.Query(
                        selection: f.Literal("0"),
                        from: articleSource,
                        where: f.Constraint(
                            articleSource.Column(Article.UserIdProperty),
                            userSource.Column(Entity.IdProperty)
                        )
                    ))
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM BlogUser AS u
WHERE u.Id > {0} AND EXISTS (
    SELECT 0
    FROM Article
    WHERE Article.UserId = u.Id
)");
        }

        [TestMethod]
        public void ORM_TableQuery_ChildrenAll()
        {
            var f = QueryFactory.Instance;
            var articleSource = f.Table(RF.Concrete<ArticleRepository>());
            var userSource = f.Table(RF.Concrete<BlogUserRepository>(), "u");

            var query = f.Query(
                from: userSource,
                where: f.And(
                    f.Constraint(
                        userSource.Column(Entity.IdProperty),
                        PropertyOperator.Greater, 0
                    ),
                    f.Not(f.Exists(f.Query(
                        selection: f.Literal("0"),
                        from: articleSource,
                        where: f.And(
                            f.Constraint(
                                articleSource.Column(Article.UserIdProperty),
                                userSource.Column(Entity.IdProperty)
                            ),
                            f.Constraint(
                                articleSource.Column(Article.IdProperty),
                                PropertyOperator.Greater, 0
                            )
                        )
                    )))
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM BlogUser AS u
WHERE u.Id > {0} AND NOT (EXISTS (
    SELECT 0
    FROM Article
    WHERE Article.UserId = u.Id AND Article.Id > {1}
))");
        }

        [TestMethod]
        public void ORM_TableQuery_SelectFromSelectResult()
        {
            var f = QueryFactory.Instance;
            var userSource = f.Table(RF.Concrete<BlogUserRepository>());

            var subQuery = f.SubQuery(
                query: f.Query(
                    from: userSource,
                    where: f.Constraint(
                        userSource.Column(Entity.IdProperty),
                        PropertyOperator.Greater, 0
                    )
                ),
                alias: "T"
            );

            var query = f.Query(
                from: subQuery,
                where: f.Constraint(
                    subQuery.Column(userSource.Column(Entity.IdProperty)),
                    PropertyOperator.Less, 100
                )
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;

            Assert.IsTrue(sql.ToString() == @"SELECT *
FROM (
    SELECT *
    FROM BlogUser
    WHERE BlogUser.Id > {0}
) AS T
WHERE T.Id < {1}");
        }

        [TestMethod]
        public void ORM_TableQuery_Paging()
        {
            var f = QueryFactory.Instance;
            var source = f.Table(RF.Concrete<ArticleRepository>());
            var pk = source.Column(Entity.IdProperty);
            var query = f.Query(
                from: source,
                where: f.Constraint(pk, PropertyOperator.Greater, 0),
                orderBy: new List<IOrderBy>
                {
                    f.OrderBy(source.Column(Article.CodeProperty))
                }
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM Article
WHERE Article.Id > {0}
ORDER BY Article.Code ASC");

            //对于已经组装完成的 IQuery 对象，ModifyToPagingTree 方法同样可以为其生成相应的分页 SqlSelect 语句。
            var pagedSelect = generator.ModifyToPagingTree(query as SqlSelect, pk as SqlColumn, new PagingInfo(3, 10));

            generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(pagedSelect);
            var pagingSql = generator.Sql;
            Assert.IsTrue(pagingSql.ToString() ==
@"SELECT TOP 10 *
FROM Article
WHERE Article.Id > {0} AND Article.Id NOT IN (
    SELECT TOP 20 Article.Id
    FROM Article
    WHERE Article.Id > {1}
    ORDER BY Article.Code ASC
)
ORDER BY Article.Code ASC");
        }

        [TestMethod]
        public void ORM_TableQuery_Paging_PageNumer1()
        {
            var f = QueryFactory.Instance;
            var source = f.Table(RF.Concrete<ArticleRepository>());
            var pk = source.Column(Entity.IdProperty);
            var query = f.Query(
                from: source,
                where: f.Constraint(pk, PropertyOperator.Greater, 0),
                orderBy: new List<IOrderBy>
                {
                    f.OrderBy(source.Column(Article.CodeProperty))
                }
            );

            var generator = new SqlServerSqlGenerator { AutoQuota = false };
            f.Generate(generator, query);
            var sql = generator.Sql;
            Assert.IsTrue(sql.ToString() ==
@"SELECT *
FROM Article
WHERE Article.Id > {0}
ORDER BY Article.Code ASC");

            //对于已经组装完成的 IQuery 对象，ModifyToPagingTree 方法同样可以为其生成相应的分页 SqlSelect 语句。
            var pagedSelect = generator.ModifyToPagingTree(query as SqlSelect, pk as SqlColumn, new PagingInfo(1, 10));

            generator = new SqlServerSqlGenerator { AutoQuota = false };
            generator.Generate(pagedSelect);
            var pagingSql = generator.Sql;
            Assert.IsTrue(pagingSql.ToString() ==
@"SELECT TOP 10 *
FROM Article
WHERE Article.Id > {0}
ORDER BY Article.Code ASC");
        }

        #endregion
    }
}