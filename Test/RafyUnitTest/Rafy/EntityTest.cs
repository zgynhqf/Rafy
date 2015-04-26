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
using Rafy.Domain.ORM.DbMigration;
using Rafy.Domain.Serialization;
using Rafy.Domain.Validation;
using Rafy.Reflection;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using Rafy.Utils;
using UT;

namespace RafyUnitTest
{
    [TestClass]
    public class EntityTest
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void ET_PersistenceStatus()
        {
            var entity = new TestUser();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.New, "刚创建的对象的状态应该是 New。");

            entity.PersistenceStatus = PersistenceStatus.Unchanged;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Unchanged);

            entity.MarkModifiedIfUnchanged();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);
        }

        [TestMethod]
        public void ET_PersistenceStatus_Delete()
        {
            var entity = new TestUser();
            entity.PersistenceStatus = PersistenceStatus.Unchanged;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Unchanged);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);

            entity.RevertDeletedStatus();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Unchanged, "之前的状态是 Unchanged");

            entity.MarkModifiedIfUnchanged();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);

            entity.RevertDeletedStatus();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified, "之前的状态是 Modified");
        }

        [TestMethod]
        public void ET_RoutedEvent()
        {
            //创建对象
            var user = new TestUser();
            var list = user.TestTreeTaskList;
            var taskRoot = list.AddNew();
            var task1 = list.AddNew();
            var task11 = list.AddNew();
            var task111 = list.AddNew();
            var task112 = list.AddNew();
            var task12 = list.AddNew();
            var task2 = list.AddNew();
            var taskRoot2 = list.AddNew();

            //关系
            task1.TreeParent = taskRoot;
            task11.TreeParent = task1;
            task111.TreeParent = task11;
            task112.TreeParent = task11;
            task12.TreeParent = task1;
            task2.TreeParent = taskRoot;

            Assert.AreEqual(taskRoot.AllTime, 0);

            task111.AllTime += 1;
            Assert.AreEqual(task11.AllTime, 1);
            Assert.AreEqual(task1.AllTime, 1);
            Assert.AreEqual(taskRoot.AllTime, 1);
            Assert.AreEqual(user.TasksTime, 1);

            task12.AllTime += 1;
            Assert.AreEqual(task1.AllTime, 2);
            Assert.AreEqual(taskRoot.AllTime, 2);
            Assert.AreEqual(user.TasksTime, 2);

            task2.AllTime += 1;
            Assert.AreEqual(task1.AllTime, 2);
            Assert.AreEqual(taskRoot.AllTime, 3);
            Assert.AreEqual(user.TasksTime, 3);

            taskRoot2.AllTime += 1;
            Assert.AreEqual(user.TasksTime, 4);

            task111.AllTime -= 1;
            Assert.AreEqual(task11.AllTime, 0);
            Assert.AreEqual(task1.AllTime, 1);
            Assert.AreEqual(taskRoot.AllTime, 2);
            Assert.AreEqual(user.TasksTime, 3);
        }

        [TestMethod]
        public void ET_AutoCollect()
        {
            //创建对象
            var user = new TestUser();
            var list = user.TestTreeTaskList;
            var taskRoot = list.AddNew();
            var task1 = list.AddNew();
            var task11 = list.AddNew();
            var task111 = list.AddNew();
            var task112 = list.AddNew();
            var task12 = list.AddNew();
            var task2 = list.AddNew();
            var taskRoot2 = list.AddNew();

            //关系
            task1.TreeParent = taskRoot;
            task11.TreeParent = task1;
            task111.TreeParent = task11;
            task112.TreeParent = task11;
            task12.TreeParent = task1;
            task2.TreeParent = taskRoot;

            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 0);

            task111.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task11.AllTimeByAutoCollect, 1);
            Assert.AreEqual(task1.AllTimeByAutoCollect, 1);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 1);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 1);

            task12.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task1.AllTimeByAutoCollect, 2);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 2);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 2);

            task2.AllTimeByAutoCollect += 1;
            Assert.AreEqual(task1.AllTimeByAutoCollect, 2);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 3);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 3);

            taskRoot2.AllTimeByAutoCollect += 1;
            Assert.AreEqual(user.TasksTimeByAutoCollect, 4);

            task111.AllTimeByAutoCollect -= 1;
            Assert.AreEqual(task11.AllTimeByAutoCollect, 0);
            Assert.AreEqual(task1.AllTimeByAutoCollect, 1);
            Assert.AreEqual(taskRoot.AllTimeByAutoCollect, 2);
            Assert.AreEqual(user.TasksTimeByAutoCollect, 3);
        }

        [TestMethod]
        public void ET_LazyRef_Serialization_OnServer()
        {
            var role = new TestRole
            {
                TestUser = new TestUser
                {
                    Id = 1,
                    Name = "TestUser"
                }
            };

            Assert.IsTrue(TestRole.TestUserProperty.DefaultMeta.Serializable, "默认在服务端，应该是可以序列化实体的。");

            var roleCloned = ObjectCloner.Clone(role);
            var loaded = roleCloned.FieldExists(TestRole.TestUserProperty);
            Assert.IsTrue(loaded, "服务端到客户端，需要序列化实体。");
        }

        [TestMethod]
        public void ET_LazyRef_Serialization_Manual()
        {
            var defaultMeta = TestRole.TestUserProperty.DefaultMeta;
            var oldValue = defaultMeta.Serializable;
            defaultMeta.Unfreeze();
            try
            {
                defaultMeta.Serializable = false;

                var role = new TestRole
                {
                    TestUser = new TestUser
                    {
                        Id = 1,
                        Name = "TestUser"
                    }
                };

                var roleCloned = ObjectCloner.Clone(role);

                var loaded = roleCloned.FieldExists(TestRole.TestUserProperty);
                Assert.IsFalse(loaded, "引用属性在 Serializable 设置为 false 时，不应该被序列化。");
            }
            finally
            {
                defaultMeta.Serializable = oldValue;
                defaultMeta.Serializable = true;
            }
        }

        [TestMethod]
        public void ET_IDomainComponent_Parent_Serialization()
        {
            var user = new TestUser
            {
                TestTreeTaskList = { new TestTreeTask() }
            };

            Assert.IsTrue(user.TestTreeTaskList.Parent == user, "新列表的 Parent 应该会自动被设置。");

            var userCloned = ObjectCloner.Clone(user);

            Assert.IsTrue(userCloned.TestTreeTaskList.Parent == userCloned, "序列化、反序列化后列表的 Parent 应该会自动被设置。");
        }

        [TestMethod]
        public void ET_Property_EnumForUI()
        {
            var entity = new TestRole
            {
                RoleType = RoleType.Administrator
            };
            var properties = (entity as ICustomTypeDescriptor).GetProperties();
            var enumProperty = properties.Find("RoleType", false);
            Assert.IsNotNull(enumProperty, "TestRole 类型上没有找到 RoleType 枚举属性。");

            Assert.IsTrue(enumProperty.PropertyType == typeof(string), "枚举属性在界面的返回值类型应该是字符串。");

            var value = enumProperty.GetValue(entity) as string;
            Assert.IsTrue(value != null && value == "管理员", "枚举属性在界面的返回值应该是枚举在 Label 标签中定义的字符串。");

            enumProperty.SetValue(entity, "一般");
            Assert.IsTrue(entity.RoleType == RoleType.Normal, "枚举属性被界面设置字符串的值时，应该转换为相应的枚举值。");
        }

        #region 属性

        [TestMethod]
        public void ET_Property_Id_Default_Int()
        {
            var user = new TestUser();
            Assert.IsTrue(user.Id == 0);
        }

        [TestMethod]
        public void ET_Property_Id_Default_Object()
        {
            var user = new GetAllCriteria();
            Assert.IsTrue(user.Id == null);
        }

        [TestMethod]
        public void ET_Property_Id_FastField()
        {
            var entity = new TestUser();
            Assert.IsTrue(entity.Id == 0);

            entity.Id = 1;
            Assert.IsTrue(entity.Id == 1);

            entity.LoadProperty(Entity.IdProperty, 2);
            Assert.IsTrue(entity.Id == 2);

            (entity as Entity).SetProperty(Entity.IdProperty, 3);
            Assert.IsTrue(entity.Id == 3);

            (entity as Entity).Id = 4;
            Assert.IsTrue(entity.Id == 4);
        }

        [TestMethod]
        public void ET_Property_AffectStatus()
        {
            var user = new TestUser();
            user.PersistenceStatus = PersistenceStatus.Unchanged;

            user.TemporaryName = "ET_Property_AffectStatus";
            Assert.IsTrue(user.PersistenceStatus == PersistenceStatus.Unchanged, "TemporaryName 不能引起实体状态的变更。");

            user.Name = "ET_Property_AffectStatus";
            Assert.IsTrue(user.IsDirty, "一般属性应该引起实体状态的变更。");
        }

        [TestMethod]
        public void ET_Property_LazyRef()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookCategory
                {
                    Name = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookCategoryId = bc.Id };
                Assert.IsTrue(book.BookCategory != null);
                Assert.IsTrue(book.BookCategory.Name == bc.Name);
            }
        }

        [TestMethod]
        public void ET_Property_LazyRef_SetEntity()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookCategory
                {
                    Name = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookCategory = bc };
                Assert.IsTrue(book.BookCategoryId == bc.Id);

                book.BookCategory = null;
                Assert.IsTrue(book.BookCategoryId == null);
            }
        }

        [TestMethod]
        public void ET_Property_LazyRef_SetId()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookCategory
                {
                    Name = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookCategoryId = bc.Id };
                Assert.IsTrue(book.BookCategory.Name == bc.Name);

                book.BookCategoryId = null;
                Assert.IsTrue(book.BookCategory == null);
            }
        }

        [TestMethod]
        public void ET_Property_LazyList()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    ChapterList =
                    {
                        new Chapter{Name = "c1"},
                        new Chapter{Name = "c2"},
                    }
                };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                Assert.IsTrue(!book2.FieldExists(Book.ChapterListProperty));
                Assert.IsTrue(book2.GetProperty(Book.ChapterListProperty) == null);
                Assert.IsTrue(book2.ChapterList.Count == 2);
            }
        }

        #endregion

        #region 属性 LOB

        [TestMethod]
        public void ET_Property_LOB_LazyValue_GetById()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                Assert.IsFalse(book2.FieldExists(Book.ContentProperty));

                var c = Logger.ThreadDbAccessedCount;
                var content = book2.Content;
                Assert.IsTrue(Logger.ThreadDbAccessedCount == c + 1);
                Assert.IsTrue(content == book.Content);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_LazyValue_GetList()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1", Content = "Book1 Long Content........." });
                repo.Save(new Book { Name = "2", Content = "Book2 Long Content........." });

                var books = repo.GetAll();
                Assert.IsFalse(books[0].FieldExists(Book.ContentProperty));
                Assert.IsFalse(books[1].FieldExists(Book.ContentProperty));

                var c = Logger.ThreadDbAccessedCount;
                Assert.IsTrue(books[0].Content == "Book1 Long Content.........");
                Assert.IsTrue(books[1].Content == "Book2 Long Content.........");
                Assert.IsTrue(Logger.ThreadDbAccessedCount == c + 2);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_UpdateWithLOB()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                book2.Name = "name changed";
                book2.Content = "Content changed";

                string updateSql = string.Empty;
                Logger.ThreadDbAccessed += (o, e) =>
                {
                    updateSql = e.Sql.ToLower();
                };

                repo.Save(book2);

                Assert.IsTrue(updateSql.Contains("update"));
                Assert.IsTrue(updateSql.Contains("content"), "LOB 属性改变时，更新语句需要同时更新该字段。");

                var book3 = repo.GetById(book.Id);
                Assert.IsTrue(book2.Content == book3.Content);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_UpdateWithoutLOB()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                book2.Name = "name changed";

                string updateSql = string.Empty;
                Logger.ThreadDbAccessed += (o, e) =>
                {
                    updateSql = e.Sql.ToLower();
                };

                repo.Save(book2);

                Assert.IsTrue(updateSql.Contains("update"));
                Assert.IsTrue(!updateSql.Contains("content"), "LOB 属性未发生改变时，更新语句不更新该字段。");
            }
        }

        [TestMethod]
        public void ET_Property_LOB_SqlSelectWithLOB()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1", Content = "Book1 Long Content........." });

                var table = repo.GetLOB(true, false);
                Assert.IsTrue(table.Columns.Find("Content") != null);
                Assert.IsTrue(table.Rows.Count == 1);
                Assert.IsTrue(table[0]["Name"].ToString() == "1");
                Assert.IsTrue(table[0]["Content"].ToString() == "Book1 Long Content.........");
            }
        }

        [TestMethod]
        public void ET_Property_LOB_SqlSelectWithoutLOB()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1", Content = "Book1 Long Content........." });

                var table = repo.GetLOB(false, false);
                Assert.IsTrue(table.Columns.Find("Content") == null);
                Assert.IsTrue(table.Rows.Count == 1);
                Assert.IsTrue(table[0]["Name"].ToString() == "1");
            }
        }

        [TestMethod]
        public void ET_Property_LOB_SqlSelectWithoutLOB_TablePrefix()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1", Content = "Book1 Long Content........." });

                var table = repo.GetLOB(false, true);
                Assert.IsTrue(table.Columns.Find("Content") == null);
                Assert.IsTrue(table.Rows.Count == 1);
                Assert.IsTrue(table[0]["Name"].ToString() == "1");
            }
        }

        #endregion

        #region 列表

        /// <summary>
        /// 当置换元素时，不能把元素的状态改变了。
        /// </summary>
        [TestMethod]
        public void ET_EntityList_SetItem()
        {
            var list = new TestUserList
            {
                new TestUser{ Name = "1" },
                new TestUser{ Name = "2" },
            };
            list.MarkSaved();
            Assert.IsTrue(list[0].PersistenceStatus == PersistenceStatus.Unchanged);
            Assert.IsTrue(list[1].PersistenceStatus == PersistenceStatus.Unchanged);

            using (list.MovingItems())
            {
                var tmp = list[0];
                list[0] = list[1];
                list[1] = tmp;
            }
            Assert.IsTrue(list[0].PersistenceStatus == PersistenceStatus.Unchanged);
            Assert.IsTrue(list[1].PersistenceStatus == PersistenceStatus.Unchanged);
            Assert.IsTrue(list[0].Name == "2");
            Assert.IsTrue(list[1].Name == "1");
        }

        /// <summary>
        /// 在列表中移除实体时，实体的 ParentList 属性应该是空。
        /// </summary>
        [TestMethod]
        public void ET_EntityList_ParentList_Remove()
        {
            var item = new TestUser();
            Assert.IsTrue(item.ParentList == null);

            var list = new TestUserList { item };
            Assert.IsTrue(item.ParentList == list);

            list.RemoveAt(0);
            Assert.IsTrue(item.ParentList == null);
        }

        #endregion

        #region 仓库

        [TestMethod]
        public void ET_Repository_CDUQ_C()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book());
                Assert.IsTrue(repo.CountAll() == 1);
            }
        }

        [TestMethod]
        public void ET_Repository_CDUQ_D()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book();
                repo.Save(book);
                Assert.IsTrue(repo.CountAll() == 1);

                book.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(book);
                Assert.IsTrue(repo.CountAll() == 0);
            }
        }

        [TestMethod]
        public void ET_Repository_CDUQ_D_Clear()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bookList = new BookList 
                { 
                    new Book(),
                    new Book(),
                    new Book()
                };
                repo.Save(bookList);
                Assert.IsTrue(repo.CountAll() == 3);

                bookList.Clear();
                repo.Save(bookList);
                Assert.IsTrue(repo.CountAll() == 0);
            }
        }

        [TestMethod]
        public void ET_Repository_CDUQ_U()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book();
                repo.Save(book);
                Assert.IsTrue(repo.CountAll() == 1);

                book.Name = "DDDDD";
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                Assert.IsTrue(book != book2);
                Assert.IsTrue(book2.Name == "DDDDD");
            }
        }

        [TestMethod]
        public void ET_Repository_Simple()
        {
            var pbsRepository = RF.Find<PBS>();
            using (RF.TransactionScope(pbsRepository))
            {
                pbsRepository.Save(new PBSType
                {
                    Name = "PBSType1",
                    PBSList =
                    {
                        new PBS { Name = "PBS1" },
                        new PBS { Name = "PBS2" },
                        new PBS { Name = "PBS3" },
                        new PBS { Name = "PBS4" },
                        new PBS { Name = "PBS5" },
                    }
                });

                var pbss = pbsRepository.GetAll();
                if (pbss.Count > 0)
                {
                    var pbsOriginal = pbss[0] as PBS;
                    var pbs = pbsRepository.GetById(pbsOriginal.Id) as PBS;
                    Assert.IsNotNull(pbs);
                    Assert.AreEqual(pbs.Id, pbsOriginal.Id);
                    Assert.AreEqual(pbs.PBSTypeId, pbsOriginal.PBSTypeId);
                    Assert.AreEqual(pbs.Name, pbsOriginal.Name);
                }
            }
        }

        [TestMethod]
        public void ET_Repository_Query_Lambda()
        {
            var repository = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repository))
            {
                repository.Save(new TestUser { Name = "AAA" });

                var list = repository.GetByName_Expression("AAA", PagingInfo.Empty);
                Assert.IsTrue(list.Count == 1);
            }
        }

        [TestMethod]
        public void ET_Repository_Query_Lambda_Count()
        {
            var repository = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repository))
            {
                repository.Save(new TestUser { Name = "AAA" });

                var count = repository.CountByName_Expression("AAA", PagingInfo.Empty);
                Assert.IsTrue(count == 1);
            }
        }

        [TestMethod]
        public void ET_Repository_TableQuery_ColumnConflict()
        {
            var repo = RF.Concrete<ChapterRepository>();
            bool success  = false;
            try
            {
                repo.QueryChapterTable(0, PagingInfo.Empty);
                success = true;
            }
            catch { }

            Assert.IsFalse(success, "列名相同时，必须报错。");
        }

        [TestMethod]
        public void ET_Repository_TableQuery()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "Book1",
                    ChapterList =
                    {
                        new Chapter{Name = "Chapter1"},
                    }
                });
                RF.Save(new Book
                {
                    Name = "Book2",
                    ChapterList =
                    {
                        new Chapter{Name = "Chapter4"},
                    }
                });

                var table = repo.QueryChapterTable(1, PagingInfo.Empty);
                Assert.IsTrue(table.Rows.Count == 2);
                var bookName = table[0].GetString("BookName");
                Assert.IsTrue(bookName.Contains("Book"));
            }
        }

        [TestMethod]
        public void ET_Repository_TableQuery_Paging()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "Book1",
                    ChapterList =
                    {
                        new Chapter{Name = "Chapter1"},
                        new Chapter{Name = "Chapter2"},
                        new Chapter{Name = "Chapter3"},
                    }
                });
                RF.Save(new Book
                {
                    Name = "Book2",
                    ChapterList =
                    {
                        new Chapter{Name = "Chapter4"},
                        new Chapter{Name = "Chapter5"},
                        new Chapter{Name = "Chapter6"},
                    }
                });

                var pi = new PagingInfo(2, 2, true);
                var table = repo.QueryChapterTable(1, pi);
                Assert.IsTrue(table.Rows.Count == 2);
                Assert.IsTrue(pi.TotalCount == 6);
                Assert.IsTrue(table[0].GetString("Name") == "Chapter4");
                Assert.IsTrue(table[1].GetString("Name") == "Chapter3");
            }
        }

        [TestMethod]
        public void ET_Repository_TableQuery_UseSqlTreeQuery()
        {
            var repo = RF.Concrete<ChapterRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(new Book
                {
                    Name = "Book1",
                    ChapterList =
                    {
                        new Chapter{Name = "01"},
                        new Chapter{Name = "02"},
                        new Chapter{Name = "11"}
                    }
                });

                var table = repo.QueryChapterBySqlTree("0", PagingInfo.Empty);
                Assert.IsTrue(table.Rows.Count == 2);
                Assert.IsTrue(table[0].GetString(Chapter.NameProperty) == "01");
                Assert.IsTrue(table[1].GetString(Chapter.NameProperty) == "02");
            }
        }

        /// <summary>
        /// 测试 MemoryEntityRepository
        /// </summary>
        [TestMethod]
        public void ET_Repository_Memory()
        {
            var repo = RF.Concrete<MemoryCustomerRepository>();
            var items = repo.GetAll();
            Assert.IsTrue(items.Count == 0);

            //添加
            var customer = new MemoryCustomer { Name = "Huqf", Age = 10 };
            repo.Save(customer);
            Assert.IsTrue(repo.CountAll() == 1);
            Assert.IsTrue(customer.PersistenceStatus == PersistenceStatus.Unchanged);

            //更新
            items = repo.GetAll();
            (items[0] as MemoryCustomer).Age = 11;
            repo.Save(items);
            items = repo.GetAll();
            Assert.IsTrue((items[0] as MemoryCustomer).Age == 11);

            //删除
            items.Clear();
            repo.Save(items);
            Assert.IsTrue(repo.CountAll() == 0);
        }

        [TestMethod]
        public void ET_Repository_Submit_Callback()
        {
            var repo = RF.Concrete<MemoryCustomerRepository>();
            var customer = new MemoryCustomer { Name = "Huqf", Version = 1 };

            repo.Save(customer);
            Assert.IsTrue(customer.Version == 3);

            customer.Age = 12;
            repo.Save(customer);
            Assert.IsTrue(customer.Version == 5);
            Assert.IsTrue(customer.PersistenceStatus == PersistenceStatus.Unchanged, "第二个版本号的添加，使用 LoadProperty");

            var customer2 = repo.GetById(customer.Id) as MemoryCustomer;
            Assert.IsTrue(customer != customer2);
            Assert.IsTrue(customer2.Version == 4, "第二个版本号的添加，在保存之后才会发生。");

            customer.PersistenceStatus = PersistenceStatus.Deleted;
            repo.Save(customer);
            Assert.IsTrue(customer.Version == 7);
        }

        [TestMethod]
        public void ET_Repository_Submit_ChildrenOnly()
        {
            var repo = RF.Concrete<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var root = new PBSType();
                var pbs = new PBS();
                root.PBSList.Add(pbs);
                repo.Save(root);

                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Unchanged);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Unchanged);

                int count = 0;
                EventHandler<Rafy.Logger.DbAccessedEventArgs> handler = (o, e) =>
                {
                    if (e.ConnectionSchema == RdbDataProvider.Get(repo).DbSetting) count++;
                };
                Logger.DbAccessed += handler;

                pbs.Name = "DDDDDDDDDD";
                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Unchanged);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Modified);

                var c = count;
                repo.Save(root);
                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Unchanged);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Unchanged);
                Assert.IsTrue(count == c + 1, "只进行了一次数据访问，即子对象的保存。");

                Logger.DbAccessed -= handler;
            }
        }

        [TestMethod]
        public void ET_Repository_Submit_ChildrenOnly_UpdateCurrent()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    ChapterList =
                    {
                        new Chapter()
                    }
                };
                repo.Save(book);

                book.ChapterList[0].Name = "DDDDD";
                var dp = repo.DataProvider as BookRepositoryDataProvider;
                try
                {
                    dp.UpdateCurrent = true;
                    var c = Logger.ThreadDbAccessedCount;
                    repo.Save(book);
                    Assert.IsTrue(Logger.ThreadDbAccessedCount == c + 2);
                }
                finally
                {
                    dp.UpdateCurrent = false;
                }
            }
        }

        [TestMethod]
        public void ET_Repository_GetChildProperties()
        {
            var repo = RF.Concrete<BookRepository>();
            var childProperties = repo.GetChildProperties();
            Assert.IsTrue(childProperties.Count == 1);
        }

        [TestMethod]
        public void ET_Repository_SaveList_Transaction()
        {
            var repo = RF.Concrete<BookLocRepository>();
            var list = repo.GetAll();
            list.Clear();
            repo.Save(list);
            var dp = repo.DataProvider as BookLocRepositoryDataProvider;
            try
            {
                dp.TestSaveListTransactionItemCount = 0;
                repo.Save(new BookLocList
                {
                    new BookLoc(),
                    new BookLoc(),
                });
                Assert.IsTrue(false, "超过一条数据，直接抛出异常。之前的数据需要回滚。");
            }
            catch (NotSupportedException) { }
            finally
            {
                dp.TestSaveListTransactionItemCount = -1;
            }

            Assert.IsTrue(repo.CountAll() == 0, "所有数据需要回滚。");
        }

        [TestMethod]
        public void ET_Repository_SplitFiles()
        {
            var repo = RF.Find<Car>();
            Assert.IsTrue(repo is CarRepository, "仓库和实体可以分离到两个项目中。");
        }

        [TestMethod]
        public void ET_Repository_DAL_ComposeDataProvider()
        {
            var repo = RF.Concrete<CarRepository>();
            if (RafyEnvironment.Location.ConnectDataDirectly)
            {
                Assert.IsTrue(repo.DataProvider is ICarDataProvider);
            }
        }

        [TestMethod]
        public void ET_Repository_DAL_Invoke()
        {
            var repo = RF.Concrete<CarRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-08") });
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-09") });
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-10") });

                var list = repo.GetByStartDate(DateTime.Parse("2014-05-08 13:30"));
                Assert.IsTrue(list.Count == 2);

                var listCount= repo.CountByStartDate(DateTime.Parse("2014-05-08 13:30"));
                Assert.IsTrue(listCount == 2);
            }
        }

        [TestMethod]
        public void ET_Repository_DAL_Replace()
        {
            var repo = RF.Concrete<CarRepository>();
            var item = repo.GetByReplacableDAL();
            Assert.IsTrue(item.Name == "ImplementationReplaced");
        }

        [TestMethod]
        public void ET_Repository_CantHasNoRepo()
        {
            bool success = false;
            try
            {
                var repo = RF.Find<NoRepoEntity>();
                success = true;
            }
            catch (TypeInitializationException)
            {
            }
            Assert.IsFalse(success, "没有编写仓库类型的实体，获取其相应的仓库时应该报错。");
        }

        #endregion

        #region 仓库 - 扩展

        [TestMethod]
        public void ET_Repository_QueryExt()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var name = "QueryExt_User";

                var user = new TestUser { Age = 10, Name = name };
                repo.Save(user);

                var userList = TestUserRepositoryExt.GetByAge(repo, 10);

                var exsit = userList.Cast<TestUser>().Any(u => u.Age == 10 && u.Name == name);
                Assert.IsTrue(exsit, "通过仓库扩展也可以查询到对应的实体。");
            }
        }

        [TestMethod]
        public void ET_Repository_QueryExt_QueryExt1ByRawRepository()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var name = "QueryExt_User";

                var user = new TestUser { Age = 10, Name = name };
                repo.Save(user);

                var criteria = new GetByAgeCriteria { Age = 10 };
                var userList = repo.GetBy(criteria);
                Assert.IsTrue(userList != null, "通过原始仓库也能调用仓库扩展中的查询。");

                var exsit = userList.Cast<TestUser>().Any(u => u.Age == 10 && u.Name == name);
                Assert.IsTrue(exsit, "通过仓库扩展也可以查询到对应的实体。");
            }
        }

        [TestMethod]
        public void ET_Repository_QueryExt_QueryExt2ByRawRepository()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var name = "QueryExt_User";

                var user = new TestUser { Age = 10, Name = name };
                repo.Save(user);

                var criteria = new GetByAge2Criteria { Age = 10 };
                var userList = repo.GetBy(criteria);
                Assert.IsTrue(userList != null, "通过原始仓库也能调用仓库扩展中的查询。");

                var exsit = userList.Cast<TestUser>().Any(u => u.Age == 10 && u.Name == name);
                Assert.IsTrue(exsit, "通过仓库扩展也可以查询到对应的实体。");
            }
        }

        [TestMethod]
        public void ET_Repository_QueryExt_QueryError()
        {
            try
            {
                var repo = RF.Concrete<TestUserRepository>();
                var userList = repo.GetBy(new NotImplementCriteria());

                Assert.IsTrue(false, "本方法没有在仓库及扩展中实现，应该抛出异常。");
            }
            catch { }
        }

        /// <summary>
        /// 在 EntityRepository 上做的扩展，在任何仓库子类上都可以调用。
        /// </summary>
        [TestMethod]
        public void ET_Repository_QueryExt_ExtendBase()
        {
            var repo = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                //查询 TestUser
                var user = new TestUser { Age = 100 };
                repo.Save(user);

                var userList = EntityRepositoryExtension.GetBySingleProperty(repo, TestUser.AgeProperty, 100);
                Assert.IsTrue(userList.Count > 0);

                var userList2 = repo.GetBy(new SinglePropertyCriteira
                {
                    PropertyName = TestUser.AgeProperty.Name,
                    Value = 100
                });
                Assert.IsTrue(userList2.Count > 0);

                //查询 TestTreeTask
                var task = new TestTreeTask { TestUser = user };
                RF.Save(task);

                var taskRepo = RF.Concrete<TestTreeTaskRepository>();
                var taskList = EntityRepositoryExtension.GetBySingleProperty(taskRepo, TestTreeTask.TestUserIdProperty, user.Id);
                Assert.IsTrue(taskList.Count > 0);

                //查询 TestAdministrator
                var adminRepo = RF.Concrete<TestAdministratorRepository>();
                var adminList = EntityRepositoryExtension.GetBySingleProperty(adminRepo, TestUser.AgeProperty, 100);
                Assert.IsTrue(adminList.Count > 0);
            }
        }

        #endregion

        #region WCF 序列化

        /// <summary>
        /// 序列化及反序列化
        /// </summary>
        [TestMethod]
        public void ET_Serialization_WCF()
        {
            var model = new Article
            {
                Code = "Code11",
                CreateDate = DateTime.Today,
            };

            //序列化。
            var serializer = new DataContractSerializer(typeof(Article));
            //var serializer = new NetDataContractSerializer();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, model);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("Article"));
            Assert.IsTrue(xml.Contains("Code11"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var model2 = (Article)serializer.ReadObject(stream);

            Assert.IsTrue(model2.Code == "Code11");
            Assert.IsTrue(model2.CreateDate == DateTime.Today);
        }

        [TestMethod]
        public void ET_Serialization_WCF_RefId()
        {
            var model = new Article
            {
                UserId = 111,
            };

            //序列化。
            var serializer = new DataContractSerializer(typeof(Article));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, model);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("Article"));
            Assert.IsTrue(xml.Contains("111"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var model2 = (Article)serializer.ReadObject(stream);

            Assert.IsTrue(model2.UserId == 111);
        }

        [TestMethod]
        public void ET_Serialization_WCF_Ref()
        {
            var model = new Article
            {
                User = new BlogUser
                {
                    Id = 111,
                    UserName = "HuQingFang"
                }
            };

            //序列化。
            var serializer = SerializationEntityGraph.CreateSerializer(model);
            //var serializer = new DataContractSerializer(typeof(Article));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, model);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("Article"));
            Assert.IsTrue(xml.Contains("<User"));
            Assert.IsTrue(xml.Contains("111"));
            Assert.IsTrue(xml.Contains("<UserName"));
            Assert.IsTrue(xml.Contains("HuQingFang"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var model2 = (Article)serializer.ReadObject(stream);

            Assert.IsTrue(model2.UserId == 111);
            Assert.IsTrue(model2.GetProperty(Article.UserProperty) != null);
            Assert.IsTrue(model2.User.UserName == "HuQingFang");
        }

        [TestMethod]
        public void ET_Serialization_WCF_List()
        {
            var model = new Book
            {
                ChapterList =
                {
                    new Chapter {
                        Id = 111,
                        Name = "Chapter1",
                    },
                    new Chapter {
                        Id = 222,
                        Name = "Chapter2",
                    },
                }
            };

            //序列化。
            var serializer = new NetDataContractSerializer();
            var stream = new MemoryStream();
            serializer.WriteObject(stream, model);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("<Book"));
            Assert.IsTrue(xml.Contains("<ChapterList"));
            Assert.IsTrue(xml.Contains("<Chapter"));
            Assert.IsTrue(xml.Contains("111"));
            Assert.IsTrue(xml.Contains("<Name"));
            Assert.IsTrue(xml.Contains("Chapter1"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var model2 = (Book)serializer.ReadObject(stream);

            Assert.IsTrue(model2.GetProperty(Book.ChapterListProperty) != null);
            Assert.IsTrue(model2.ChapterList.Count == 2);
            Assert.IsTrue(model2.ChapterList[0].Id == 111);
            Assert.IsTrue(model2.ChapterList[0].Name == "Chapter1");
        }

        #endregion

        #region 主键

        /// <summary>
        /// 测试当 Id 不是主键，而使用其它的属性作为主键时的情况。
        /// </summary>
        [TestMethod]
        public void ET_Id_NotPrimaryKey()
        {
            var repo = RF.Concrete<BuildingRepository>();
            using (RF.TransactionScope(repo))
            {
                var model = new Building();
                model.Name = "A";
                repo.Save(model);
                Assert.IsTrue(model.Id > 0);
                Assert.IsTrue(repo.CountAll() == 1);

                model = new Building();
                model.Name = "B";
                repo.Save(model);
                Assert.IsTrue(model.Id > 0);
                Assert.IsTrue(repo.CountAll() == 2);

                model = new Building();
                model.Name = "A";
                try
                {
                    repo.Save(model);
                    Assert.IsTrue(false, "主键不能重复插入。");
                }
                catch (DbException) { }
            }
        }

        #endregion

        #region 验证

        [TestMethod]
        public void ET_Validation()
        {
            var user = new TestUser();
            var brokenRules = user.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
            //由于当前没有界面层元数据，所以错误字符串中应该是属性的名称。
            Assert.AreEqual(brokenRules[0].Description, "[NotEmptyCode] 里没有输入值。");
        }

        [TestMethod]
        public void ET_Validation_NotDuplicateRule()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = new Book { Code = "01" };
                repo.Save(entity);

                var entity2 = new Book { Code = "01" };
                var brokenRules = entity2.Validate();
                Assert.AreEqual(brokenRules.Count, 1, "[Code] 的值必须唯一。");
            }
        }

        [TestMethod]
        public void ET_Validation_NotDuplicateRule_MultiProperties()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = new Book { Name = "IT秘籍", Author = "胡庆访" };
                repo.Save(entity);

                var entity2 = new Book { Name = "IT秘籍", Author = "胡庆访" };
                var brokenRules = entity2.Validate();
                Assert.AreEqual(brokenRules.Count, 1, "Name 和 Author 的值必须唯一。");

                entity2 = new Book { Name = "IT秘籍", Author = "徐丹丹" };
                brokenRules = entity2.Validate();
                Assert.AreEqual(brokenRules.Count, 0, "Name 和 Author 的值必须唯一。");
            }
        }

        [TestMethod]
        public void ET_Validation_NotDuplicateRule_Deleted()
        {
            var repo = RF.Concrete<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = new Book { Code = "01" };
                repo.Save(entity);

                var entity2 = repo.GetById(entity.Id);
                var error = entity2.Validate();
                Assert.AreEqual(error.Count, 0, "刚查询出来的实体可以直接通过 NotDuplicateRule 的验证。");

                entity2.PersistenceStatus = PersistenceStatus.Deleted;
                error = entity2.Validate();
                Assert.AreEqual(error.Count, 0, "删除状态的实体可以直接通过 NotDuplicateRule 的验证。");
            }
        }

        [TestMethod]
        public void ET_Validation_NotUsedByReferenceRule()
        {
            var repo = RF.Concrete<BookCategoryRepository>();
            using (RF.TransactionScope(repo))
            {
                var cate = new BookCategory { Code = "01" };
                repo.Save(cate);

                var book = new Book { BookCategory = cate };
                RF.Save(book);

                var rules = cate.Validate();
                Assert.IsTrue(rules.Count == 1);
                Assert.IsTrue(rules[0].Rule.ValidationRule is NotUsedByReferenceRule);
            }
        }

        [TestMethod]
        public void ET_Validation_Clear()
        {
            var entity = new TestUserQueryCriteria();
            var brokenRules = entity.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
        }

        [TestMethod]
        public void ET_Validation_Criteria()
        {
            var entity = new TestUserQueryCriteria();
            var brokenRules = entity.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
            Assert.AreEqual(brokenRules[0].Description, "[Name] 里没有输入值。");
        }

        #endregion

        ////由于 LazyEntityRef 类删除后，所以不再可以在运行时控制是否可序列化，本测试不再可用。
        //[TestMethod]
        //public void ET_LazyRef_Serialization_Manual()
        //{
        //    var role = new TestRole
        //    {
        //        TestUser = new TestUser
        //        {
        //            Id = 1,
        //            Name = "TestUser"
        //        }
        //    };

        //    var lazyRef = role.GetLazyRef(TestRole.TestUserRefProperty);

        //    lazyRef.SerializeEntity = true;
        //    var roleCloned = ObjectCloner.Clone(role);
        //    var lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
        //    Assert.IsTrue(lazyRefCloned.LoadedOrAssigned, "需要序列化实体。");

        //    lazyRef.SerializeEntity = false;
        //    roleCloned = ObjectCloner.Clone(role);
        //    lazyRefCloned = roleCloned.GetLazyRef(TestRole.TestUserRefProperty);
        //    Assert.IsFalse(lazyRefCloned.LoadedOrAssigned, "不需要序列化实体。");
        //}

        //[TestMethod]
        //public void ET_Repository_Override()
        //{
        //    RF.OverrideRepository<ContractBudget, RealContractBudget>();
        //    var r = RF.Concreate<ContractBudgetRepository>();
        //    Assert.AreEqual(r.GetType().Name, "RealContractBudgetRepository");
        //    Assert.AreEqual(r.New().GetType(), typeof(RealContractBudget));

        //    var r2 = RF.Create<ContractBudget>();
        //    Assert.AreEqual(r2.New().GetType(), typeof(RealContractBudget));
        //}

        //[TestMethod]
        //public void ET_AddBatch()
        //{
        //    //以下测试是无法直接通过的，因为无法插入相同的数据

        //    //var pbsTypes = RF.Create<PBSType>();
        //    //var pbsType1 = pbsTypes.GetAll()[3] as PBSType;
        //    //pbsType1.PBSBQItemsLoader.WaitForLoading();
        //    //pbsType1.PBSNormItemsLoader.WaitForLoading();
        //    //pbsType1.PBSPropertiesLoader.WaitForLoading();
        //    //var reader = new EntityChldrenBatchReader(pbsType1);
        //    //var dic = reader.Read();

        //    //foreach (var dicItem in dic)
        //    //{
        //    //    var repository = RF.Create(dicItem.Key);
        //    //    repository.AddBatch(dicItem.Value);
        //    //}
        //}
    }
}