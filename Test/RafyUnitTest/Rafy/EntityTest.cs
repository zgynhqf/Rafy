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
using Rafy.UnitTest;
using Rafy.ManagedProperty;
using Rafy.DataPortal;
using Rafy.Accounts;

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

            entity.PersistenceStatus = PersistenceStatus.Saved;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Saved);

            entity.MarkModifiedIfSaved();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);
        }

        [TestMethod]
        public void ET_PersistenceStatus_Modified()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = repo.New();
                repo.Save(entity);

                entity.Name = "name changed.";
                Assert.AreEqual(PersistenceStatus.Modified, entity.PersistenceStatus);
                repo.Save(entity);

                var entity2 = repo.GetById(entity.Id);
                Assert.AreEqual(entity.Name, entity2.Name);
            }
        }

        [TestMethod]
        public void ET_PersistenceStatus_Modified_SetAsPropertyChanged()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = repo.New();
                repo.Save(entity);

                entity.Name = "name changed.";
                Assert.AreEqual(PersistenceStatus.Modified, entity.PersistenceStatus);
            }
        }

        [TestMethod]
        public void ET_PersistenceStatus_New()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = repo.New();

                Assert.IsTrue(user.IsNew);

                repo.Save(user);
                Assert.IsTrue(!user.IsNew);
            }
        }

        [TestMethod]
        public void ET_PersistenceStatus_New_IsDirty()
        {
            var user = new TestUser();

            Assert.IsTrue(user.PersistenceStatus == PersistenceStatus.New);
            Assert.IsTrue(user.IsNew);
            Assert.IsTrue(user.IsDirty);
        }

        //[TestMethod]
        //public void ET_PersistenceStatus_New_SetNewWillResetId()
        //{
        //    var item = new TestUser { Id = 111 };
        //    item.PersistenceStatus = PersistenceStatus.Unchanged;

        //    item.PersistenceStatus = PersistenceStatus.New;

        //    Assert.AreEqual(0, item.Id, "设置实体的状态为 new 时，需要重置其 Id。");
        //}

        [TestMethod]
        public void ET_PersistenceStatus_Delete()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = repo.New();
                repo.Save(user);

                user.PersistenceStatus = PersistenceStatus.Deleted;
                Assert.IsTrue(user.IsDeleted);
                repo.Save(user);

                Assert.AreEqual(PersistenceStatus.Saved, user.PersistenceStatus);
            }
        }

        [TestMethod]
        public void ET_PersistenceStatus_Delete_IsDirty()
        {
            var entity = new TestUser();

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);
            Assert.IsTrue(entity.IsDirty);
            Assert.IsTrue(entity.IsDirty);
        }

        [TestMethod]
        public void ET_PersistenceStatus_Delete_RevertDeletedStatus()
        {
            var entity = new TestUser();
            entity.PersistenceStatus = PersistenceStatus.Saved;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Saved);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);

            (entity as IEntityWithStatus).RevertDeletedStatus();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Saved, "之前的状态是 Unchanged");

            entity.MarkModifiedIfSaved();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified);

            entity.PersistenceStatus = PersistenceStatus.Deleted;
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Deleted);

            (entity as IEntityWithStatus).RevertDeletedStatus();
            Assert.IsTrue(entity.PersistenceStatus == PersistenceStatus.Modified, "之前的状态是 Modified");
        }

        [TestMethod]
        public void ET_PersistenceStatus_Delete_Update0Row()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = repo.New();
                repo.Save(user);

                user.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(user);

                Assert.AreEqual(PersistenceStatus.Saved, user.PersistenceStatus);

                var rowsAffected = -1;
                EventHandler<DbAccessedEventArgs> handler = (o, e) =>
                {
                    rowsAffected = (int)e.Result;
                };
                DbAccesserInterceptor.DbAccessed += handler;
                user.Name = "name changed.";
                repo.Save(user);
                Assert.AreEqual(0, rowsAffected, "由于数据已经删除，所以这里影响的行号为 0。");
                DbAccesserInterceptor.DbAccessed -= handler;
            }
        }

        [TestMethod]
        public void ET_PersistenceStatus_Delete_Reinsert()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var user = repo.New();
                repo.Save(user);
                var oldId = user.Id;

                user.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(user);

                user.PersistenceStatus = PersistenceStatus.New;
                user.Id = -1;
                repo.Save(user);
                Assert.AreNotEqual(oldId, user.Id, "重新插入此实体，需要生成新的行。");
                Assert.AreEqual(PersistenceStatus.Saved, user.PersistenceStatus);
            }
        }

        /// <summary>
        /// 在列表中的实体，不论其的状态如何变换，都不会影响 IEntityList 中的项的个数，除非直接操作 IEntityList。
        /// </summary>
        [TestMethod]
        public void ET_PersistenceStatus_Delete_ParentListItemsNotChanged()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var users = new TestUserList
                {
                    new TestUser(),
                    new TestUser()
                };
                repo.Save(users);

                users[0].PersistenceStatus = PersistenceStatus.Deleted;
                users.RemoveAt(1);
                repo.Save(users);

                Assert.AreEqual(1, users.Count);
                Assert.AreEqual(0, users.DeletedList.Count);
                Assert.AreEqual(PersistenceStatus.Saved, users[0].PersistenceStatus);
                var oldId = users[0].Id;

                users[0].PersistenceStatus = PersistenceStatus.New;
                users[0].Id = -1;
                repo.Save(users);

                Assert.AreEqual(1, users.Count);
                Assert.AreEqual(0, users.DeletedList.Count);
                Assert.AreEqual(PersistenceStatus.Saved, users[0].PersistenceStatus);
                Assert.AreNotEqual(oldId, users[0].Id, "重新插入此实体，需要生成新的行。");
            }
        }

        //[TestMethod]
        //public void ET_PersistenceStatus_Delete_SavedAsNew_Reinsert()
        //{
        //    var repo = RF.ResolveInstance<TestUserRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        var item = new TestUser();
        //        repo.Save(item);

        //        var oldId = item.Id;

        //        item.PersistenceStatus = PersistenceStatus.Deleted;
        //        repo.Save(item);

        //        Assert.AreEqual(0, item.Id, "再次插入已经删除的数据时，Id 应该会重置。");

        //        repo.Save(item);
        //        Assert.AreNotEqual(oldId, item.Id, "再次插入已经删除的数据时，Id 应该会重新生成。");
        //    }
        //}

        //[TestMethod]
        //public void ET_PersistenceStatus_Delete_SavedAsNew_ListClear()
        //{
        //    var repo = RF.ResolveInstance<TestUserRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        var items = new TestUserList
        //        {
        //            new TestUser(),
        //            new TestUser(),
        //            new TestUser()
        //        };
        //        repo.Save(items);

        //        items.RemoveAt(0);
        //        Assert.AreEqual(1, items.DeletedList.Count);
        //        items[0].PersistenceStatus = PersistenceStatus.Deleted;
        //        repo.Save(items);

        //        Assert.AreEqual(0, items.DeletedList.Count, "保存已删除数据的数据列表时，已经删除的数据需要从 List 中删除。");
        //        Assert.AreEqual(1, items.Count, "保存已删除数据的数据列表时，已经删除的数据需要从 List 中删除。");
        //        Assert.AreEqual(PersistenceStatus.Unchanged, items[0].PersistenceStatus);
        //    }
        //}

        //[TestMethod]
        //public void ET_PersistenceStatus_Delete_SavedAsNew_Aggt()
        //{
        //    var repo = RF.ResolveInstance<TestUserRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        var user = new TestUser
        //        {
        //            TestRoleList =
        //            {
        //                new TestRole()
        //            }
        //        };
        //        repo.Save(user);
        //        Assert.IsTrue(repo.CountAll() == 1);

        //        user.PersistenceStatus = PersistenceStatus.Deleted;
        //        repo.Save(user);

        //        Assert.AreEqual(user.PersistenceStatus, PersistenceStatus.New, "实体被删除后，状态应该为 New。");
        //        Assert.AreEqual(user.TestRoleList[0].PersistenceStatus, PersistenceStatus.New, "聚合实体被删除后，整个聚合中所有实体的状态应该为 New。");
        //    }
        //}

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

        #region 属性

        [TestMethod]
        public void ET_Property_Enum()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var user = new TestUser { Name = "DDDDD" };
                RF.Save(user);

                var repo = RF.ResolveInstance<TestRoleRepository>();
                var entity = new TestRole
                {
                    TestUser = user,
                    RoleType = RoleType.Administrator
                };
                repo.Save(entity);

                var entity2 = repo.GetById(entity.Id);
                Assert.AreEqual(entity.RoleType, entity2.RoleType);
            }
        }

        [TestMethod]
        public void ET_Property_Decimal()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                Yacht car = new Yacht()
                {
                    DecimalValue = (decimal)999999.99
                };
                var repo = RF.ResolveInstance<YachtRepository>();
                repo.Save(car);

                long id = car.Id;
                var newCar = repo.GetById(id);

                Assert.AreEqual(newCar.DecimalValue, car.DecimalValue);
            }
        }

        [TestMethod]
        public void ET_Property_Float()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                Yacht car = new Yacht()
                {
                    FloatValue = (float)16.6,
                };
                var repo = RF.ResolveInstance<YachtRepository>();
                repo.Save(car);

                long id = car.Id;
                var newCar = repo.GetById(id);

                Assert.AreEqual(newCar.FloatValue, car.FloatValue);
            }
        }

        [TestMethod]
        public void ET_Property_Byte()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                Yacht car = new Yacht()
                {
                    ByteValue = (byte)1
                };
                var repo = RF.ResolveInstance<YachtRepository>();
                repo.Save(car);

                long id = car.Id;
                var newCar = repo.GetById(id);

                Assert.AreEqual(newCar.ByteValue, car.ByteValue);
            }
        }

        [TestMethod]
        public void ET_Property_Bytes()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book
                {
                    Bytes = Encoding.UTF8.GetBytes("test content")
                };
                repo.Save(book);

                var newBook = repo.GetById(book.Id);

                Assert.AreEqual("test content", Encoding.UTF8.GetString(newBook.Bytes));
            }
        }

        [TestMethod]
        public void ET_Property_Boolean()
        {
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                var book = new Book()
                {
                    IsSoldOut = true
                };
                var repo = RF.ResolveInstance<BookRepository>();
                repo.Save(book);

                var newBook = repo.GetById(book.Id);
                Assert.AreEqual(newBook.IsSoldOut, true);

                newBook.IsSoldOut = false;
                repo.Save(newBook);

                var newBook2 = repo.GetById(book.Id);
                Assert.AreEqual(newBook2.IsSoldOut, false);
            }
        }

        /// <summary>
        /// https://technet.microsoft.com/zh-cn/library/ms172424(v=sql.110).aspx
        /// </summary>
        [TestMethod]
        public void ET_Property_DateTimeOffset()
        {
            using (var tran = RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                Yacht car = new Yacht()
                {
                    DateTimeOffsetValue = DateTime.Parse("1030-1-1")
                };

                if (tran.DbSetting.ProviderName.Contains("SqlServerCe"))
                {
                    car.DateTimeOffsetValue = DateTime.Parse("1753-1-1");
                }

                var repo = RF.ResolveInstance<YachtRepository>();
                repo.Save(car);

                long id = car.Id;
                var newCar = repo.GetById(id);

                Assert.AreEqual(newCar.DateTimeOffsetValue, car.DateTimeOffsetValue);
            }
        }

        [TestMethod]
        public void ET_Property_Enum_ForUI()
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

        [TestMethod]
        public void ET_Property_Id_Default_Int()
        {
            var user = new TestUser();
            Assert.IsTrue(user.Id == 0);
        }

        [TestMethod]
        public void ET_Property_Id_Default_Object()
        {
            var user = new CommonQueryCriteria();
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
            user.PersistenceStatus = PersistenceStatus.Saved;

            user.TemporaryName = "ET_Property_AffectStatus";
            Assert.IsTrue(user.PersistenceStatus == PersistenceStatus.Saved, "TemporaryName 不能引起实体状态的变更。");

            user.Name = "ET_Property_AffectStatus";
            Assert.IsTrue(user.IsDirty, "一般属性应该引起实体状态的变更。");
        }

        [TestMethod]
        public void ET_Property_AffectStatus_RefEntityNotAffectStatus()
        {
            var book = new Book
            {
                Id = 1,
                BookCategoryId = 1
            };
            book.PersistenceStatus = PersistenceStatus.Saved;

            book.BookCategory = new BookCategory
            {
                Id = 1,
            };
            Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus, "BookCategoryId 没有变，虽然 BookCategory 从 null 变为 cate1，但是状态也不会改变。");

            book.BookCategory = new BookCategory
            {
                Id = 1,
            };
            Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus, "BookCategoryId 没有变，虽然 BookCategory 改变了对象，但是状态也不会改变。");

            book.BookCategory = new BookCategory
            {
                Id = 2,
            };
            Assert.AreEqual(PersistenceStatus.Modified, book.PersistenceStatus, "BookCategoryId 改变，状态也跟着改变。");
        }

        [TestMethod]
        public void ET_Property_LazyRef_LazyLoad()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
        public void ET_Property_LazyRef_ValueProperty_LazyLoad()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookLoc
                {
                    Code = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookLocCode = bc.Code };
                Assert.IsTrue(book.BookLoc != null);
                Assert.IsTrue(book.BookLoc.Code == bc.Code);
            }
        }

        [TestMethod]
        public void ET_Property_LazyRef_ValueProperty_SetEntity()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookLoc
                {
                    Code = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookLoc = bc };
                Assert.IsTrue(book.BookLocCode == bc.Code);

                book.BookLoc = null;
                Assert.IsTrue(string.IsNullOrEmpty(book.BookLocCode));
            }
        }

        [TestMethod]
        public void ET_Property_LazyRef_ValueProperty_SetValue()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var bc = new BookLoc
                {
                    Code = "bc1"
                };
                RF.Save(bc);

                var book = new Book { BookLocCode = bc.Code };
                Assert.IsTrue(book.BookLoc.Code == bc.Code);

                book.BookLocCode = null;
                Assert.IsTrue(book.BookLoc == null);
            }
        }

        [TestMethod]
        public void ET_Property_LazyList()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
                Assert.IsTrue(!book2.HasLocalValue(Book.ChapterListProperty));
                Assert.IsTrue(book2.GetProperty<ChapterList>(Book.ChapterListProperty) == null);
                Assert.IsTrue(book2.ChapterList.Count == 2);
            }
        }

        [TestMethod]
        public void ET_Property_ToDataTable()
        {
            var list = new BookList
            {
                new Book{Name = "book1", Code = "001"},
                new Book{Name = "book2", Code = "002"},
                new Book{Name = "book3", Code = "003"},
            };

            var table = list.ToDataTable();

            Assert.AreEqual(table.Rows.Count, 3);
            Assert.AreEqual(table.Rows[0]["Name"], "book1");
            Assert.AreEqual(table.Rows[0]["Code"], "001");
            Assert.AreEqual(table.Rows[1]["Name"], "book2");
            Assert.AreEqual(table.Rows[1]["Code"], "002");
            Assert.AreEqual(table.Rows[2]["Name"], "book3");
            Assert.AreEqual(table.Rows[2]["Code"], "003");
        }

        #endregion

        #region 属性 LOB

        [TestMethod]
        public void ET_Property_LOB_LazyValue_GetById()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                Assert.IsFalse(book2.HasLocalValue(Book.ContentProperty));

                var c = DbAccesserInterceptor.ThreadDbAccessedCount;
                var content = book2.Content;
                Assert.IsTrue(DbAccesserInterceptor.ThreadDbAccessedCount == c + 1);
                Assert.IsTrue(content == book.Content);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_LazyValue_GetList()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book { Name = "1", Content = "Book1 Long Content........." });
                repo.Save(new Book { Name = "2", Content = "Book2 Long Content........." });

                var books = repo.GetAll();
                Assert.IsFalse(books[0].HasLocalValue(Book.ContentProperty));
                Assert.IsFalse(books[1].HasLocalValue(Book.ContentProperty));

                var c = DbAccesserInterceptor.ThreadDbAccessedCount;
                Assert.IsTrue(books[0].Content == "Book1 Long Content.........");
                Assert.IsTrue(books[1].Content == "Book2 Long Content.........");
                Assert.IsTrue(DbAccesserInterceptor.ThreadDbAccessedCount == c + 2);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_UpdateWithLOB()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                book2.Name = "name changed";
                book2.Content = "Content changed";

                repo.Save(book2);

                string updateSql = DbAccesserInterceptor.ThreadLastDbAccessedArgs.Sql.ToLower();
                Assert.IsTrue(updateSql.Contains("update"));
                Assert.IsTrue(updateSql.Contains("content"), "LOB 属性改变时，更新语句需要同时更新该字段。");

                var book3 = repo.GetById(book.Id);
                Assert.IsTrue(book2.Content == book3.Content);
            }
        }

        [TestMethod]
        public void ET_Property_LOB_UpdateWithoutLOB()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var dp = RdbDataProvider.Get(repo);

                var book = new Book { Name = "1", Content = "Book1 Long Content........." };
                repo.Save(book);

                var book2 = repo.GetById(book.Id);
                book2.Name = "name changed";

                repo.Save(book2);

                string updateSql = DbAccesserInterceptor.ThreadLastDbAccessedArgs.Sql.ToLower();
                Assert.IsTrue(updateSql.Contains("update"));
                Assert.IsTrue(!updateSql.Contains("content"), "LOB 属性未发生改变时，更新语句不更新该字段。");
            }
        }

        [TestMethod]
        public void ET_Property_LOB_SqlSelectWithLOB()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
        /// 编译期语法测试：测试开发者可以正常使用 Linq 扩展方法
        /// </summary>
        [TestMethod]
        public void ET_EntityList_CanUseLinq()
        {
            var list = new UserList();
            list.FirstOrDefault(e => e.RealName == "");
        }

        [TestMethod]
        public void ET_EntityList_CanUseLinq_Interface()
        {
            var list = new UserList() as IEntityList;
            list.Linq.FirstOrDefault(e => e.Id != null);
        }

        /// <summary>
        /// 编译期语法测试：测试开发者可以使用 foreach，且使用 var 即可，而不需要强制转换为 Entity
        /// </summary>
        [TestMethod]
        public void ET_EntityList_CanUseForeach()
        {
            var list = new UserList();
            foreach (var item in list)
            {
                item.RealName = "";
            }
        }

        [TestMethod]
        public void ET_EntityList_CanUseForeach_Interface()
        {
            var list2 = new UserList() as IEntityList;
            foreach (var item in list2)
            {
                item.Id = null;
            }
        }

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
            Assert.IsTrue(list[0].PersistenceStatus == PersistenceStatus.Saved);
            Assert.IsTrue(list[1].PersistenceStatus == PersistenceStatus.Saved);

            using (list.MovingItems())
            {
                var tmp = list[0];
                list[0] = list[1];
                list[1] = tmp;
            }
            Assert.IsTrue(list[0].PersistenceStatus == PersistenceStatus.Saved);
            Assert.IsTrue(list[1].PersistenceStatus == PersistenceStatus.Saved);
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
            Assert.IsTrue((item as IEntity).ParentList == null);

            var list = new TestUserList();
            list.ResetItemParent = true;
            list.Add(item);
            Assert.IsTrue((item as IEntity).ParentList == list);

            list.RemoveAt(0);
            Assert.IsTrue((item as IEntity).ParentList == null);
        }

        #endregion

        #region 仓库

        /// <summary>
        /// 当开发者没有为实体定义仓库时，自动生成一个内部的仓库类型。
        /// </summary>
        [TestMethod]
        public void ET_Repository_AutoGenearteDefaultRepository()
        {
            var repo = RF.Find<NoRepoEntity>();
            Assert.IsNotNull(repo);
        }

        //[TestMethod]
        //public void ET_Repository_CantHasNoRepo()
        //{
        //    bool success = false;
        //    try
        //    {
        //        var repo = RF.Find<NoRepoEntity>();
        //        success = true;
        //    }
        //    catch (TypeInitializationException)
        //    {
        //    }
        //    Assert.IsFalse(success, "没有编写仓库类型的实体，获取其相应的仓库时应该报错。");
        //}

        [TestMethod]
        public void ET_Repository_CDUQ_C()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Book());
                Assert.IsTrue(repo.CountAll() == 1);
            }
        }

        [TestMethod]
        public void ET_Repository_CDUQ_D()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
                RF.Save(new PBSType
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
        public void ET_Repository_Aggt_Delete()
        {
            var pbsTypeRepository = RF.Find<PBSType>();
            using (RF.TransactionScope(pbsTypeRepository))
            {
                var pbsType = new PBSType
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
                };
                pbsTypeRepository.Save(pbsType);

                var pbsTypeInDb = pbsTypeRepository.GetById(pbsType.Id);
                Assert.IsNotNull(pbsTypeInDb);

                pbsTypeInDb.PersistenceStatus = PersistenceStatus.Deleted;
                pbsTypeRepository.Save(pbsTypeInDb);

                var pbsRepository = RF.Find<PBS>();
                var pbss = pbsRepository.GetByParentId(pbsType.Id);
                Assert.AreEqual(0, pbss.Count, "删除聚合父实体，应该级联删除其聚合子。");
            }
        }

        [TestMethod]
        public void ET_Repository_ChangeDbSetting()
        {
            var repo = RF.ResolveInstance<ChapterRepository>();
            var dataProvider = (repo.DataProvider as RdbDataProvider);
            var dbSetting = dataProvider.DbSetting;

            //两个仓储同时变更数据源
            var newBookRepo = RF.ResolveInstance<BookRepository>();
            using (RdbDataProvider.RedirectDbSetting(UnitTestEntityRepositoryDataProvider.DbSettingName, UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
            {
                Assert.AreEqual(RdbDataProvider.Get(newBookRepo).DbSetting.Name, UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate);
                Assert.AreEqual(RdbDataProvider.Get(repo).DbSetting.Name, UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate);
            }

            //还原
            Assert.AreEqual(dbSetting, dataProvider.DbSetting);
        }

        [TestMethod]
        public void ET_Repository_RedirectDbSetting()
        {
            var bookRepo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            {
                bookRepo.Save(new Book());
                Assert.AreEqual(1, bookRepo.CountAll());

                using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                {
                    using (RdbDataProvider.RedirectDbSetting(UnitTestEntityRepositoryDataProvider.DbSettingName,
                            UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                    {
                        Assert.AreEqual(0, bookRepo.CountAll());

                        bookRepo.Save(new Book());
                        Assert.AreEqual(1, bookRepo.CountAll());
                    }
                }

                Assert.AreEqual(1, bookRepo.CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_RedirectDbSetting_Batch()
        {
            var bookRepo = RF.ResolveInstance<BookRepository>();
            var chapterRepo = RF.ResolveInstance<ChapterRepository>();
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName))
            using (RF.TransactionScope(UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
            {
                bookRepo.Save(new Book
                {
                    ChapterList =
                    {
                        new Chapter()
                    }
                });
                Assert.AreEqual(1, bookRepo.CountAll());
                Assert.AreEqual(1, chapterRepo.CountAll());

                using (RdbDataProvider.RedirectDbSetting(UnitTestEntityRepositoryDataProvider.DbSettingName, UnitTestEntityRepositoryDataProvider.DbSettingName_Duplicate))
                {
                    Assert.AreEqual(0, bookRepo.CountAll());
                    Assert.AreEqual(0, chapterRepo.CountAll());
                }
            }
        }

        /// <summary>
        /// 只查询部分列时，也可以进行实体的查询。
        /// </summary>
        [TestMethod]
        public void ET_Repository_Query_SpecifyColumns()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var name = "admin";

                var user = new TestUser { Age = 100, Name = name, LoginName = "loginName" };
                repo.Save(user);

                var dbUser = repo.GetOnlyName(name);

                Assert.AreEqual(name, dbUser.Name);

                dbUser.Disable(TestUser.LoginNameProperty, false);
                dbUser.Disable(TestUser.AgeProperty, false);
                Assert.AreEqual(null, dbUser.LoginName);
                Assert.AreEqual(10, dbUser.Age);
            }
        }

        //[TestMethod]
        //public void ET_Repository_Query_Lambda()
        //{
        //    var repository = RF.ResolveInstance<TestUserRepository>();
        //    using (RF.TransactionScope(repository))
        //    {
        //        repository.Save(new TestUser { Name = "AAA" });

        //        var list = repository.GetByName_Expression("AAA", PagingInfo.Empty);
        //        Assert.IsTrue(list.Count == 1);
        //    }
        //}

        //[TestMethod]
        //public void ET_Repository_Query_Lambda_Count()
        //{
        //    var repository = RF.ResolveInstance<TestUserRepository>();
        //    using (RF.TransactionScope(repository))
        //    {
        //        repository.Save(new TestUser { Name = "AAA" });

        //        var count = repository.CountByName_Expression("AAA", PagingInfo.Empty);
        //        Assert.IsTrue(count == 1);
        //    }
        //}

        [TestMethod]
        public void ET_Repository_QueryTable_ColumnConflict()
        {
            var repo = RF.ResolveInstance<ChapterRepository>();
            bool success = false;
            try
            {
                repo.QueryChapterTable(0, PagingInfo.Empty);
                success = true;
            }
            catch { }

            Assert.IsFalse(success, "列名相同时，必须报错。");
        }

        [TestMethod]
        public void ET_Repository_QueryTable()
        {
            var repo = RF.ResolveInstance<ChapterRepository>();
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
        public void ET_Repository_QueryTable_Paging()
        {
            var repo = RF.ResolveInstance<ChapterRepository>();
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
        public void ET_Repository_QueryTable_UseSqlTreeQuery()
        {
            var repo = RF.ResolveInstance<ChapterRepository>();
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

        [TestMethod]
        public void ET_Repository_LinqGetBySingleBoolean_Where()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var root1 = new Invoice() { Code = "root1", IsDefault = true };
                var child1 = new InvoiceItem() { Amount = 1000 };
                root1.InvoiceItemList.Add(child1);
                repo.Save(root1);

                var root2 = new Invoice() { Code = "root2", IsDefault = false };
                var child2 = new InvoiceItem() { Amount = 2000 };
                root2.InvoiceItemList.Add(child2);
                repo.Save(root2);

                var list1 = repo.LinqByIsDefaultBoolean(true).Concrete().ToList();
                Assert.IsTrue(list1.Count == 1);
                Assert.IsTrue(list1.First().Code == "root1");
                Assert.IsTrue(list1.First().IsDefault);

                var list2 = repo.LinqByIsDefaultBoolean(false).Concrete().ToList();
                Assert.IsTrue(list2.Count == 1);
                Assert.IsTrue(list2.First().Code == "root2");
                Assert.IsTrue(!list2.First().IsDefault);
            }
        }

        [TestMethod]
        public void ET_Repository_LinqGetByAggregateChildrenSingleBoolean_Any_Exist()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var root1 = new Invoice() { Code = "root1", IsDefault = true };
                var child1_1 = new InvoiceItem() { Amount = 1000, IsDefault = true };
                var child1_2 = new InvoiceItem() { Amount = 2000, IsDefault = true };
                root1.InvoiceItemList.Add(child1_1);
                root1.InvoiceItemList.Add(child1_2);
                repo.Save(root1);

                var root2 = new Invoice() { Code = "root2", IsDefault = true };
                var child2_1 = new InvoiceItem() { Amount = 3000, IsDefault = true };
                var child2_2 = new InvoiceItem() { Amount = 4000, IsDefault = false };
                root2.InvoiceItemList.Add(child2_1);
                root2.InvoiceItemList.Add(child2_2);
                repo.Save(root2);

                var root3 = new Invoice() { Code = "root3", IsDefault = false };
                var child3_1 = new InvoiceItem() { Amount = 5000, IsDefault = false };
                var child3_2 = new InvoiceItem() { Amount = 6000, IsDefault = false };
                root3.InvoiceItemList.Add(child3_1);
                root3.InvoiceItemList.Add(child3_2);
                repo.Save(root3);

                var list1 = repo.LinqByItemListIsDefaultBooleanAny(true).Concrete().ToList();
                Assert.IsTrue(list1.Count == 2);
                Assert.IsTrue(list1.First().Code == "root1");
                Assert.IsTrue(list1.First().IsDefault);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().Count == 2);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().First().Amount == 1000);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().First().IsDefault);
                Assert.IsTrue(list1.Last().Code == "root2");
                Assert.IsTrue(list1.Last().IsDefault);
                Assert.IsTrue(list1.Last().InvoiceItemList.Concrete().ToList().Count == 2);
                Assert.IsTrue(list1.Last().InvoiceItemList.Concrete().ToList().Last().Amount == 4000);
                Assert.IsTrue(!list1.Last().InvoiceItemList.Concrete().ToList().Last().IsDefault);

                var list2 = repo.LinqByItemListIsDefaultBooleanAny(false).Concrete().ToList();
                Assert.IsTrue(list2.Count == 2);
                Assert.IsTrue(list2.Last().Code == "root3");
                Assert.IsTrue(!list2.Last().IsDefault);
                Assert.IsTrue(list2.Last().InvoiceItemList.Concrete().ToList().Count == 2);
                Assert.IsTrue(list2.Last().InvoiceItemList.Concrete().ToList().First().Amount == 5000);
                Assert.IsTrue(!list2.Last().InvoiceItemList.Concrete().ToList().First().IsDefault);
            }
        }

        [TestMethod]
        public void ET_Repository_LinqGetByAggregateChildrenSingleBoolean_Any_Not_Exist()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var root1 = new Invoice() { Code = "root1", IsDefault = true };
                var child1 = new InvoiceItem() { Amount = 1000, IsDefault = true };
                root1.InvoiceItemList.Add(child1);
                repo.Save(root1);

                var root2 = new Invoice() { Code = "root2", IsDefault = true };
                var child2 = new InvoiceItem() { Amount = 2000, IsDefault = true };
                root2.InvoiceItemList.Add(child2);
                repo.Save(root2);

                var list = repo.LinqByItemListIsDefaultBooleanAny(false).Concrete().ToList();
                Assert.IsTrue(list.Count == 0);
            }
        }

        [TestMethod]
        public void ET_Repository_LinqGetByAggregateChildrenSingleBoolean_All_Exist()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var root1 = new Invoice() { Code = "root1", IsDefault = true };
                var child1_1 = new InvoiceItem() { Amount = 1000, IsDefault = true };
                var child1_2 = new InvoiceItem() { Amount = 2000, IsDefault = true };
                root1.InvoiceItemList.Add(child1_1);
                root1.InvoiceItemList.Add(child1_2);
                repo.Save(root1);

                var root2 = new Invoice() { Code = "root2", IsDefault = false };
                var child2_1 = new InvoiceItem() { Amount = 3000, IsDefault = false };
                var child2_2 = new InvoiceItem() { Amount = 4000, IsDefault = false };
                root2.InvoiceItemList.Add(child2_1);
                root2.InvoiceItemList.Add(child2_2);
                repo.Save(root2);

                var root3 = new Invoice() { Code = "root3", IsDefault = false };
                var child3_1 = new InvoiceItem() { Amount = 5000, IsDefault = true };
                var child3_2 = new InvoiceItem() { Amount = 6000, IsDefault = false };
                root3.InvoiceItemList.Add(child3_1);
                root3.InvoiceItemList.Add(child3_2);
                repo.Save(root3);

                var list1 = repo.LinqByItemListIsDefaultBooleanAll(true).Concrete().ToList();
                Assert.IsTrue(list1.Count == 1);
                Assert.IsTrue(list1.First().Code == "root1");
                Assert.IsTrue(list1.First().IsDefault);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().Count == 2);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().First().Amount == 1000);
                Assert.IsTrue(list1.First().InvoiceItemList.Concrete().ToList().First().IsDefault);

                var list2 = repo.LinqByItemListIsDefaultBooleanAll(false).Concrete().ToList();
                Assert.IsTrue(list2.Count == 1);
                Assert.IsTrue(list2.First().Code == "root2");
                Assert.IsTrue(!list2.First().IsDefault);
                Assert.IsTrue(list2.First().InvoiceItemList.Concrete().ToList().Count == 2);
                Assert.IsTrue(list2.First().InvoiceItemList.Concrete().ToList().First().Amount == 3000);
                Assert.IsTrue(!list2.First().InvoiceItemList.Concrete().ToList().First().IsDefault);
            }
        }

        [TestMethod]
        public void ET_Repository_LinqGetByAggregateChildrenSingleBoolean_All_Not_Exist()
        {
            var repo = RF.ResolveInstance<InvoiceRepository>();
            using (RF.TransactionScope(repo))
            {
                var root1 = new Invoice() { Code = "root1", IsDefault = true };
                var child1_1 = new InvoiceItem() { Amount = 1000, IsDefault = false };
                var child1_2 = new InvoiceItem() { Amount = 2000, IsDefault = true };
                root1.InvoiceItemList.Add(child1_1);
                root1.InvoiceItemList.Add(child1_2);
                repo.Save(root1);

                var root2 = new Invoice() { Code = "root2", IsDefault = false };
                var child2_1 = new InvoiceItem() { Amount = 3000, IsDefault = true };
                var child2_2 = new InvoiceItem() { Amount = 4000, IsDefault = false };
                root2.InvoiceItemList.Add(child2_1);
                root2.InvoiceItemList.Add(child2_2);
                repo.Save(root2);

                var root3 = new Invoice() { Code = "root3", IsDefault = false };
                var child3_1 = new InvoiceItem() { Amount = 5000, IsDefault = true };
                var child3_2 = new InvoiceItem() { Amount = 6000, IsDefault = false };
                root3.InvoiceItemList.Add(child3_1);
                root3.InvoiceItemList.Add(child3_2);
                repo.Save(root3);

                var list1 = repo.LinqByItemListIsDefaultBooleanAll(false).Concrete().ToList();
                Assert.IsTrue(list1.Count == 0);
            }
        }

        /// <summary>
        /// 测试 MemoryEntityRepository
        /// </summary>
        [TestMethod]
        public void ET_Repository_Memory()
        {
            var repo = RF.ResolveInstance<MemoryCustomerRepository>();
            var items = repo.GetAll();
            Assert.IsTrue(items.Count == 0);

            //添加
            var customer = new MemoryCustomer { Name = "Huqf", Age = 10 };
            repo.Save(customer);
            Assert.IsTrue(repo.CountAll() == 1);
            Assert.IsTrue(customer.PersistenceStatus == PersistenceStatus.Saved);

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
            var repo = RF.ResolveInstance<MemoryCustomerRepository>();
            var customer = new MemoryCustomer { Name = "Huqf", Version = 1 };

            repo.Save(customer);
            Assert.IsTrue(customer.Version == 3);

            customer.Age = 12;
            repo.Save(customer);
            Assert.IsTrue(customer.Version == 5);
            Assert.IsTrue(customer.PersistenceStatus == PersistenceStatus.Saved, "第二个版本号的添加，使用 LoadProperty");

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
            var repo = RF.ResolveInstance<PBSTypeRepository>();
            using (RF.TransactionScope(repo))
            {
                var root = new PBSType();
                var pbs = new PBS();
                root.PBSList.Add(pbs);
                repo.Save(root);

                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Saved);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Saved);

                int count = 0;
                EventHandler<Rafy.Data.DbAccessEventArgs> handler = (o, e) =>
                {
                    if (e.ConnectionSchema == RdbDataProvider.Get(repo).DbSetting) count++;
                };
                DbAccesserInterceptor.DbAccessing += handler;

                pbs.Name = "DDDDDDDDDD";
                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Saved);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Modified);

                var c = count;
                repo.Save(root);
                Assert.IsTrue(root.PersistenceStatus == PersistenceStatus.Saved);
                Assert.IsTrue(pbs.PersistenceStatus == PersistenceStatus.Saved);
                Assert.IsTrue(count == c + 1, "只进行了一次数据访问，即子对象的保存。");

                DbAccesserInterceptor.DbAccessing -= handler;
            }
        }

        [TestMethod]
        public void ET_Repository_Submit_ChildrenOnly_UpdateCurrent()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
                    var c = DbAccesserInterceptor.ThreadDbAccessedCount;
                    repo.Save(book);
                    Assert.AreEqual(DbAccesserInterceptor.ThreadDbAccessedCount, c + 2);
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
            var repo = RF.ResolveInstance<BookRepository>();
            var childProperties = repo.GetChildProperties();
            Assert.IsTrue(childProperties.Count == 1);
        }

        [TestMethod]
        public void ET_Repository_SaveList_Transaction()
        {
            var repo = RF.ResolveInstance<BookLocRepository>();
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
            var repo = RF.ResolveInstance<CarRepository>();
            if (DataPortalApi.ConnectDataDirectly)
            {
                Assert.IsTrue(repo.DataProvider is ICarDataProvider);
            }
        }

        [TestMethod]
        public void ET_Repository_DAL_Invoke()
        {
            var repo = RF.ResolveInstance<CarRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-08") });
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-09") });
                repo.Save(new Car { AddTime = DateTime.Parse("2014-05-10") });

                var list = repo.GetByStartDate(DateTime.Parse("2014-05-08 13:30"));
                Assert.IsTrue(list.Count == 2);

                var listCount = repo.CountByStartDate(DateTime.Parse("2014-05-08 13:30"));
                Assert.IsTrue(listCount == 2);
            }
        }

        [TestMethod]
        public void ET_Repository_DAL_Replace()
        {
            var repo = RF.ResolveInstance<CarRepository>();
            var item = repo.GetByReplacableDAL();
            Assert.IsTrue(item.Name == "ImplementationReplaced");
        }

        #endregion

        #region 批量导入

        internal static readonly int BATCH_IMPORT_DATA_SIZE = ConfigurationHelper.GetAppSettingOrDefault("RafyUnitTest.BatchImportDataSize", 100);

        /// <summary>
        /// 批量导入需要支持事务回滚
        /// </summary>
        [TestMethod]
        public void ET_Repository_BatchImport_Transaction()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var bookRepo = RF.ResolveInstance<BookRepository>();
            var chapterRepo = RF.ResolveInstance<ChapterRepository>();
            Assert.AreEqual(0, bookRepo.CountAll());
            using (RF.TransactionScope(bookRepo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book
                    {
                        ChapterList =
                        {
                            new Chapter(),
                            new Chapter(),
                        }
                    };
                    books.Add(book);
                }

                var importer = bookRepo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, bookRepo.CountAll());
                Assert.AreEqual(size * 2, chapterRepo.CountAll());
            }
            Assert.AreEqual(0, chapterRepo.CountAll());
            Assert.AreEqual(0, bookRepo.CountAll());
        }

        /// <summary>
        /// 支持设置批量的大小
        /// </summary>
        [TestMethod]
        public void ET_Repository_BatchImport_BatchSize()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = (int)(2.345 * BATCH_IMPORT_DATA_SIZE);

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.BatchSize = 1000;
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());

                for (int i = 0; i < size; i++)
                {
                    var book = books[i];
                    Assert.IsTrue(book.Id > 0);
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_TreeEntity()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var veryChild = new Folder() as ITreeEntity;
                var list = new FolderList();
                for (int i = 0; i < size; i++)
                {
                    var folder = new Folder
                    {
                        TreeChildren =
                        {
                            new Folder
                            {
                                TreeChildren =
                                {
                                    veryChild as Entity,
                                }
                            }
                        }
                    };
                    list.Add(folder);
                }

                var importer = repo.CreateImporter();
                importer.Save(list);

                Assert.AreEqual(size, repo.CountAll());

                for (int i = 0; i < size; i++)
                {
                    var item = list[i];
                    Assert.IsTrue(item.Id > 0);
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_Identity()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    book.Id = i + 1;
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());

                for (int i = 0; i < size; i++)
                {
                    var book = books[i];
                    Assert.IsTrue(book.Id > 0, " Identity 手动赋值");
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_CLOB()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var longContent = new string('X', 20000);

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    book.Content = longContent;
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_BLOB()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var longContent = new byte[20000];

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    book.Bytes = longContent;
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_Aggt()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book
                    {
                        ChapterList =
                        {
                            new Chapter(),
                            new Chapter(),
                        }
                    };
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());
                Assert.AreEqual(size * 2, RF.ResolveInstance<ChapterRepository>().CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_Status()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }

                repo.CreateImporter().Save(books);

                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_C_Status_Aggt()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book
                    {
                        ChapterList =
                        {
                            new Chapter(),
                            new Chapter(),
                        }
                    };
                    books.Add(book);
                }

                repo.CreateImporter().Save(books);

                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                    foreach (var chapter in book.ChapterList)
                    {
                        Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                    }
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_U()
        {
            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());

                for (int i = 0; i < size; i++)
                {
                    books[i].Code = i.ToString();
                }
                importer.Save(books);

                var res = repo.GetByIdList(new object[] { books[0].Id, books[books.Count - 1].Id });
                Assert.AreEqual("0", res[0].Code);
                Assert.AreEqual((size - 1).ToString(), res[res.Count - 1].Code);
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_U_Aggt()
        {
            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book
                    {
                        ChapterList =
                        {
                            new Chapter(),
                            new Chapter(),
                        }
                    };
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);

                Assert.AreEqual(size, repo.CountAll());
                Assert.AreEqual(size * 2, RF.ResolveInstance<ChapterRepository>().CountAll());

                for (int i = 0; i < size; i++)
                {
                    books[i].Code = i.ToString();
                    books[i].ChapterList[0].Name = i.ToString();
                }
                importer.Save(books);

                var res = repo.GetByIdList(new object[] { books[0].Id, books[books.Count - 1].Id });
                Assert.AreEqual("0", res[0].Code);
                Assert.AreEqual("0", res[0].ChapterList[0].Name);
                Assert.AreEqual((size - 1).ToString(), res[res.Count - 1].Code);
                Assert.AreEqual((size - 1).ToString(), res[res.Count - 1].ChapterList[0].Name);
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_U_Status()
        {
            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }

                var importer = repo.CreateImporter();
                importer.Save(books);
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                }

                for (int i = 0; i < size; i++)
                {
                    books[i].Code = i.ToString();
                }
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Modified, book.PersistenceStatus);
                }

                importer.Save(books);
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                }
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_U_Status_Aggt()
        {
            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book
                    {
                        ChapterList =
                        {
                            new Chapter(),
                            new Chapter(),
                        }
                    };
                    books.Add(book);
                }
                var importer = repo.CreateImporter();
                importer.Save(books);
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                    foreach (var chapter in book.ChapterList)
                    {
                        Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                    }
                }

                for (int i = 0; i < size; i++)
                {
                    books[i].Code = i.ToString();
                    books[i].ChapterList[0].Name = i.ToString();
                }
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Modified, book.PersistenceStatus);
                    Assert.AreEqual(PersistenceStatus.Modified, book.ChapterList[0].PersistenceStatus);
                }

                importer.Save(books);
                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.Saved, book.PersistenceStatus);
                    Assert.AreEqual(PersistenceStatus.Saved, book.ChapterList[0].PersistenceStatus);
                }
            }
        }

        /// <summary>
        /// 被冗余属性在批量更新时，在框架层面也能自动更新其对应的冗余属性。
        /// </summary>
        [TestMethod]
        public void __ET_Repository_BatchImport_CDU_U_Redundancy_UpdateB()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<ARepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new AList();
                for (int i = 0; i < size; i++)
                {
                    var item = new A { Name = i.ToString() };
                    list.Add(item);
                }

                var importer = repo.CreateImporter();
                importer.Save(list);

                var a = list[0];
                var b = new B { A = a };
                RF.Save(b);

                for (int i = 0; i < size; i++)
                {
                    list[i].Name = "New Name";
                }
                importer.Save(list);

                var res = repo.GetByIdList(new object[] { list[0].Id, list[list.Count - 1].Id });
                Assert.AreEqual("New Name", res[0].Name);
                Assert.AreEqual("New Name", res[res.Count - 1].Name);

                var b2 = RF.ResolveInstance<BRepository>().GetById(b.Id);
                Assert.AreEqual("New Name", b2.AName);
            }
        }

        /// <summary>
        /// 被冗余属性在批量更新时，在框架层面也能自动更新其对应的冗余属性。
        /// </summary>
        [TestMethod]
        public void __ET_Repository_BatchImport_CDU_U_Redundancy_UpdateC()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<ARepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new AList();
                for (int i = 0; i < size; i++)
                {
                    var item = new A { Name = i.ToString() };
                    list.Add(item);
                }

                var importer = repo.CreateImporter();
                importer.Save(list);

                var a = list[0];
                var b = new B { A = a };
                RF.Save(b);

                var c = new C { B = b };
                RF.Save(c);

                for (int i = 0; i < size; i++)
                {
                    list[i].Name = "New Name";
                }
                importer.Save(list);

                var res = repo.GetByIdList(new object[] { list[0].Id, list[list.Count - 1].Id });
                Assert.AreEqual("New Name", res[0].Name);
                Assert.AreEqual("New Name", res[res.Count - 1].Name);

                var b2 = RF.ResolveInstance<BRepository>().GetById(b.Id);
                Assert.AreEqual("New Name", b2.AName);

                var c2 = RF.ResolveInstance<CRepository>().GetById(c.Id) as C;
                Assert.AreEqual("New Name", c2.AName);
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_D()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }
                repo.CreateImporter().Save(books);

                books.Clear();
                repo.CreateImporter().Save(books);

                Assert.AreEqual(0, repo.CountAll());
            }
        }

        [TestMethod]
        public void ET_Repository_BatchImport_CDU_D_Status()
        {
            if (DbMigrationTest.IsTestDbSQLite()) return;

            int size = BATCH_IMPORT_DATA_SIZE;

            var repo = RF.ResolveInstance<BookRepository>();
            using (RF.TransactionScope(repo))
            {
                var books = new BookList();
                for (int i = 0; i < size; i++)
                {
                    var book = new Book();
                    books.Add(book);
                }
                repo.CreateImporter().Save(books);

                books.Clear();
                repo.CreateImporter().Save(books);

                for (int i = 0, c = books.Count; i < c; i++)
                {
                    var book = books[i];
                    Assert.AreEqual(PersistenceStatus.New, book.PersistenceStatus);
                }
            }
        }

        #endregion

        #region 仓库 - 扩展

        [TestMethod]
        public void ET_Repository_QueryExt()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                var name = "QueryExt_User";

                var user = new TestUser { Age = 10, Name = name };
                repo.Save(user);

                var userList = repo.Extension<TestUserRepositoryExt>().GetByAge(10);

                var exsit = userList.Cast<TestUser>().Any(u => u.Age == 10 && u.Name == name);
                Assert.IsTrue(exsit, "通过仓库扩展也可以查询到对应的实体。");
            }
        }

        [TestMethod]
        public void ET_Repository_QueryExt_QueryExt1ByRawRepository()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
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
            var repo = RF.ResolveInstance<TestUserRepository>();
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
            var success = false;
            try
            {
                var repo = RF.ResolveInstance<TestUserRepository>();
                var userList = repo.GetBy(new NotImplementCriteria());

                success = true;
            }
            catch { }

            Assert.IsFalse(success, "本方法没有在仓库及扩展中实现，应该抛出异常。");
        }

        /// <summary>
        /// 在 EntityRepository 上做的扩展，在任何仓库子类上都可以调用。
        /// </summary>
        [TestMethod]
        public void ET_Repository_QueryExt_ExtendBase()
        {
            var repo = RF.ResolveInstance<TestUserRepository>();
            using (RF.TransactionScope(repo))
            {
                //查询 TestUser
                var user = new TestUser { Age = 100 };
                repo.Save(user);

                var userList = repo.Extension<EntityRepositoryExtension>().GetBySingleProperty(new ConcreteProperty(TestUser.AgeProperty), 100);
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

                var taskRepo = RF.ResolveInstance<TestTreeTaskRepository>();
                var taskList = taskRepo.Extension<EntityRepositoryExtension>().GetBySingleProperty(new ConcreteProperty(TestTreeTask.TestUserIdProperty), user.Id);
                Assert.IsTrue(taskList.Count > 0);

                //查询 TestAdministrator
                var adminRepo = RF.ResolveInstance<TestAdministratorRepository>();
                var adminList = adminRepo.Extension<EntityRepositoryExtension>().GetBySingleProperty(new ConcreteProperty(TestUser.AgeProperty), 100);
                Assert.IsTrue(adminList.Count > 0);
            }
        }

        [TestMethod]
        public void ET_Repository_QueryExt_GetByObjectCriteria_Exception()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            try
            {
                repo.GetBy(new NonsenceCriteria());
                Assert.IsFalse(true, "这里需要发生异常，因为给定的参数并没有在仓库中找到对应的方法。");
            }
            catch (InvalidProgramException) { }
        }

        [Serializable]
        private class NonsenceCriteria { }

        #endregion

        #region 主键

        /// <summary>
        /// 测试当 Id 不是主键，而使用其它的属性作为主键时的情况。
        /// </summary>
        [TestMethod]
        public void ET_Id_NotPrimaryKey()
        {
            var setting = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            if (DbSetting.IsOracleProvider(setting))
            {
                return;
            }

            var repo = RF.Find<Building>();
            using (RF.TransactionScope(repo))
            {
                var model = new Building();
                model.Name = "A";
                repo.Save(model);
                //Assert.IsTrue(model.Id > 0);
                Assert.IsTrue(repo.CountAll() == 1);

                model = new Building();
                model.Name = "B";
                repo.Save(model);
                // Assert.IsTrue(model.Id > 0);
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
            Assert.AreEqual(brokenRules[0].Description, "[UT.TestUser.NotEmptyCode] 里没有输入值。");
        }

        [TestMethod]
        public void ET_Validation_Criteria()
        {
            var entity = new TestUserQueryCriteria();
            var brokenRules = entity.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
        }

        [TestMethod]
        public void ET_Validation_Criteria_Message()
        {
            var entity = new TestUserQueryCriteria();
            var brokenRules = entity.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
            Assert.AreEqual(brokenRules[0].Description, "[UT.TestUserQueryCriteria.Name] 里没有输入值。");
        }

        [TestMethod]
        public void ET_Validation_NotDuplicateRule()
        {
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
            var repo = RF.ResolveInstance<BookRepository>();
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
        public void ET_Validation_NotUsedByReferenceRule_EntityStatusScope_Delete()
        {
            var repo = RF.ResolveInstance<BookCategoryRepository>();
            using (RF.TransactionScope(repo))
            {
                var cate = new BookCategory { Code = "01" };
                repo.Save(cate);

                var book = new Book { BookCategory = cate };
                RF.Save(book);

                var rules = cate.Validate();
                Assert.IsTrue(rules.Count == 0);

                cate.PersistenceStatus = PersistenceStatus.Deleted;
                rules = cate.Validate();
                Assert.IsTrue(rules.Count == 1);
                Assert.IsTrue(rules[0].Rule.ValidationRule is NotUsedByReferenceRule);
            }
        }

        #endregion

        #region Clone

        [TestMethod]
        public void ET_Clone()
        {
            var b = new B { Name = "b1", Id = 1 };
            var b2 = new B();
            b2.Clone(b);

            Assert.AreEqual("b1", b2.Name);
            Assert.AreEqual(0, b2.Id);
        }

        [TestMethod]
        public void ET_Clone_NormalProperties_NotIncludeReadOnlyProperty()
        {
            var a = new TestUser { Name = "name", Age = 20 };
            var b = new TestUser();

            b.Clone(a, new CloneOptions(CloneActions.NormalProperties));

            Assert.AreEqual("name", b.Name);
            Assert.AreEqual(20, b.Age);

            Assert.IsFalse(b.HasLocalValue(TestUser.ReadOnlyNameAgeProperty));
        }

        [TestMethod]
        public void ET_Clone_NormalProperties_NotIncludeDefaultValue()
        {
            var a = new TestUser();
            Assert.AreEqual("DefaultName", a.Name);

            var b = new TestUser();
            b.Clone(a, new CloneOptions(CloneActions.NormalProperties));

            Assert.AreEqual("DefaultName", b.Name);
            Assert.IsFalse(b.HasLocalValue(TestUser.NameProperty));
        }

        [TestMethod]
        public void ET_Clone_ReadDbRow()
        {
            var a = new A { Name = "a1", Id = 1 };
            var b = new B { Name = "b1", Id = 1, A = a };
            var b2 = new B();
            b2.Clone(b, CloneOptions.ReadDbRow());

            Assert.AreEqual("b1", b2.Name);
            Assert.AreEqual(1, b2.Id);
            Assert.AreEqual("a1", b2.ANameRef);
            Assert.IsNull(b2.GetProperty<Entity>(B.AProperty));
        }

        [TestMethod]
        public void ET_Clone_NewSingleEntity()
        {
            var a = new A { Name = "a1", Id = 1 };
            var b = new B { Name = "b1", Id = 1, A = a };
            var b2 = new B();
            b2.Clone(b, CloneOptions.NewSingleEntity());

            Assert.AreEqual(b.Name, b2.Name);
            Assert.AreEqual(0, b2.Id);
            Assert.AreEqual(a.Name, b2.ANameRef);
            Assert.IsNotNull(b2.GetProperty(B.AProperty));
        }

        [TestMethod]
        public void ET_Clone_NewSingleEntity_ParentRef()
        {
            var pbsType = new PBSType { Id = 1 };
            var pbs = new PBS { PBSType = pbsType };

            var pbs2 = new PBS();
            pbs2.Clone(pbs);

            Assert.IsNotNull(pbs2.GetProperty<Entity>(PBS.PBSTypeProperty));
        }

        [TestMethod]
        public void ET_Clone_NewComposition()
        {
            var pbsType = new PBSType { Id = 1, Name = "PBS1" };
            pbsType.PBSList.Add(new PBS { Id = 1, Name = "pbs1.1 a" });
            pbsType.PBSList.Add(new PBS { Id = 2, Name = "pbs1.2 a" });
            (pbsType as IDirtyAware).MarkSaved();

            var pbsType2 = new PBSType();

            pbsType2.Clone(pbsType, CloneOptions.NewComposition());

            Assert.AreEqual(pbsType.Name, pbsType2.Name);
            Assert.IsTrue(pbsType2.IsNew);
            Assert.AreEqual(2, pbsType2.PBSList.Count);
            Assert.AreEqual("pbs1.1 a", pbsType2.PBSList[0].Name);
            Assert.IsTrue(pbsType2.PBSList[0].IsNew);
            Assert.AreEqual("pbs1.2 a", pbsType2.PBSList[1].Name);
            Assert.IsTrue(pbsType2.PBSList[1].IsNew);
        }

        [TestMethod]
        public void ET_Clone_ChildrenRecur()
        {
            var pbsType = new PBSType { Id = 1, Name = "PBS1" };
            pbsType.PBSList.Add(new PBS { Id = 1, Name = "pbs1.1 a" });
            pbsType.PBSList.Add(new PBS { Id = 2, Name = "pbs1.2 a" });
            (pbsType as IDirtyAware).MarkSaved();

            var pbsType2 = new PBSType { Id = 1, Name = "PBS1 another" };
            var pbs22 = new PBS { Id = 2, Name = "pbs1.2 b" };
            pbsType2.PBSList.Add(pbs22);
            (pbsType2 as IDirtyAware).MarkSaved();

            var options = new CloneOptions(CloneActions.NormalProperties | CloneActions.ChildrenRecur);
            pbsType2.Clone(pbsType, options);

            Assert.AreEqual(pbsType.Name, pbsType2.Name);
            Assert.AreEqual(2, pbsType2.PBSList.Count);
            Assert.AreSame(pbs22, pbsType2.PBSList[0], "该对象并没有直接删除到 Deleted 列表中。");
            Assert.AreEqual("pbs1.1 a", pbs22.Name);
            Assert.AreEqual("pbs1.2 a", pbsType2.PBSList[1].Name, "不按 Id 来复制，而是直接根据位置来复制。");
        }

        [TestMethod]
        public void ET_Clone_LoadProperty()
        {
            var pbsType = new PBSType { Id = 1, Name = "PBS1" };
            pbsType.PBSList.Add(new PBS { Id = 1, Name = "pbs1.1 a" });
            pbsType.PBSList.Add(new PBS { Id = 2, Name = "pbs1.2 a" });
            (pbsType as IDirtyAware).MarkSaved();

            var pbsType2 = new PBSType { Id = 1, Name = "PBS1 another" };
            var pbs21 = new PBS { Id = 1, Name = "pbs1.1 b" };
            pbsType2.PBSList.Add(pbs21);
            (pbsType2 as IDirtyAware).MarkSaved();
            Assert.IsFalse(pbsType2.IsDirty);

            var options = new CloneOptions(CloneActions.IdProperty | CloneActions.NormalProperties | CloneActions.ChildrenRecur);
            options.Method = CloneValueMethod.LoadProperty;

            pbsType2.Clone(pbsType, options);

            Assert.IsFalse(pbsType2.IsDirty);
        }

        /// <summary>
        /// 可以使用 Clone 方法复制一个树节点。
        /// </summary>
        [TestMethod]
        public void ET_Clone_Tree_Struc()
        {
            var list = new FolderList
            {
                new Folder
                {
                    TreeChildren =
                    {
                        new Folder(),
                        new Folder(),
                    }
                },
                new Folder
                {
                    TreeChildren =
                    {
                        new Folder(),
                    }
                },
            };

            var list2 = new FolderList();
            list2.Clone(list, CloneOptions.NewComposition());

            Assert.IsTrue(list2.Count == 2);
            var a = list2[0];
            Assert.IsTrue(a.TreeChildren.IsLoaded && a.TreeChildren.Count == 2);
            foreach (ITreeEntity treeChild in a.TreeChildren)
            {
                Assert.IsTrue(treeChild.IsTreeParentLoaded && treeChild.TreeParent == a);
            }
            var b = list2[1];
            Assert.IsTrue(b.TreeChildren.IsLoaded && b.TreeChildren.Count == 1);
        }

        [TestMethod]
        public void ET_Clone_TreeIndex_TreePId()
        {
            var repo = RF.ResolveInstance<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var rawList = new FolderList
                {
                    new Folder
                    {
                        Name = "1.",
                        TreeChildren =
                        {
                            new Folder
                            {
                                Name = "1.1.",
                            },
                             new Folder
                            {
                                Name = "1.2.",
                            }
                        }
                    }
                };

                repo.Save(rawList);

                var options = CloneOptions.NewComposition();
                var toCloneList = new FolderList();
                toCloneList.Clone(rawList, options);

                repo.Save(toCloneList);

                Assert.AreEqual("001.", toCloneList[0].TreeIndex);
                Assert.AreEqual("001.001.", toCloneList[0].TreeChildren[0].TreeIndex);
                Assert.AreEqual("001.002.", toCloneList[0].TreeChildren[1].TreeIndex);

                Assert.AreNotEqual(rawList[0].TreeChildren[0].TreePId, toCloneList[0].TreeChildren[0].TreePId);
            }
        }

        #endregion

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