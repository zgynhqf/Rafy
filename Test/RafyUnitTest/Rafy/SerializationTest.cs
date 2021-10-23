/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211019
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211019 20:24
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
using Rafy.UnitTest;
using Rafy.ManagedProperty;
using Rafy.Serialization.Mobile;

namespace RafyUnitTest
{
    [TestClass]
    public class SerializationTest
    {
        [ClassInitialize]
        public static void SrlzT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region 二进制序列化

        [TestMethod]
        public void SrlzT_Binary_MPT()
        {
            var e1 = new TestUser();
            e1.PersistenceStatus = PersistenceStatus.Saved;
            e1.Age = 15;
            e1._mySelfReference = e1;
            TestUserExt.SetUserCode(e1, "TestUserExt_UserCode");

            Assert.AreEqual(e1.Validate().Count, 1);

            #region 复制对象（序列化+反序列化）

            var e2 = ObjectCloner.Clone(e1);

            //实体直接定义的字段
            Assert.AreEqual(e1._ageNonserailizable, 15);
            Assert.AreEqual(e1._ageSerailizable, 15);
            Assert.AreEqual(e2._ageSerailizable, 15);
            Assert.AreEqual(e2._ageNonserailizable, 0);
            Assert.AreEqual(e2._mySelfReference, e2);
            Assert.AreEqual(e2._now, DateTime.Today);

            //Rafy属性
            Assert.IsTrue(e2.IsDirty);
            Assert.IsFalse(e2.IsNew);
            Assert.IsFalse(e2.IsDeleted);

            //一般属性
            Assert.AreEqual(e2.Age, 15);
            Assert.AreEqual(e1.Age, 15);

            //默认属性
            Assert.AreEqual(e2.Id, e1.Id);
            Assert.AreEqual(e2.Name, "DefaultName");
            Assert.AreEqual(e1.Name, "DefaultName");

            #endregion

            #region 检测具体的序列化的值

            var si = new SerializationContainerContext(new SerializationInfoContainer(0), null);
            si.IsProcessingState = true;
            e1.CastTo<IMobileObject>().SerializeState(si);

            var list = si.States.Keys.ToArray();

            Assert.IsTrue(list.Contains("Age"), "Age 是属性值，需要序列化");
            Assert.IsTrue(list.Contains("TestUserExt_UserCode"), "UserCode 是扩展属性值，需要序列化");
            Assert.IsTrue(!list.Contains("Name"), "Name 是默认值，不需要序列化");
            Assert.IsTrue(!list.Contains("Id"), "Id 是默认值，不需要序列化");

            #endregion
        }

        [TestMethod]
        public void SrlzT_Binary_LazyRef_OnServer()
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
            var loaded = roleCloned.HasLocalValue(TestRole.TestUserProperty);
            Assert.IsTrue(loaded, "服务端到客户端，需要序列化实体。");
        }

        [TestMethod]
        public void SrlzT_Binary_LazyRef_Manual()
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

                var loaded = roleCloned.HasLocalValue(TestRole.TestUserProperty);
                Assert.IsFalse(loaded, "引用属性在 Serializable 设置为 false 时，不应该被序列化。");
            }
            finally
            {
                defaultMeta.Serializable = oldValue;
                defaultMeta.Serializable = true;
            }
        }

        [TestMethod]
        public void SrlzT_Binary_IDomainComponent_Parent_Serialization()
        {
            var user = new TestUser
            {
                TestTreeTaskList = { new TestTreeTask() }
            };

            Assert.IsTrue(user.TestTreeTaskList.Parent == user, "新列表的 Parent 应该会自动被设置。");

            var userCloned = ObjectCloner.Clone(user);

            Assert.IsTrue(userCloned.TestTreeTaskList.Parent == userCloned, "序列化、反序列化后列表的 Parent 应该会自动被设置。");
        }

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

        /// <summary>
        /// 属性的变更状态，需要支持序列化和反序列化。
        /// </summary>
        [TestMethod]
        public void SrlzT_Binary_MPT_ChangedStatus()
        {
            var user = new TestUser();
            user.Name = "1";
            user.MarkPropertiesUnchanged();
            user.Age = 100;

            var fields = ManagedPropertyTest.GetChangedProperties(user);
            Assert.AreEqual(1, fields.Count);
            Assert.AreSame(TestUser.AgeProperty, fields[0].Property);

            var user2 = ObjectCloner.Clone(user);

            fields = ManagedPropertyTest.GetChangedProperties(user2);
            Assert.AreEqual(1, fields.Count);
            Assert.AreSame(TestUser.AgeProperty, fields[0].Property);
        }

        [TestMethod]
        public void SrlzT_Binary_LoadOptions()
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

        [TestMethod]
        public void SrlzT_Binary_ORM_Query_EmptyPaging()
        {
            var cloned = ObjectCloner.Clone(PagingInfo.Empty);
            Assert.IsTrue(cloned == PagingInfo.Empty, "EmptyPagingInfo 只有在单例情况下，才能使用它作为空的分页参数。");
        }

        [TestMethod]
        public void SrlzT_Binary_TET_ByBit()
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

            list = ObjectCloner.Clone(list);

            Assert.IsTrue(list.Count == 2);
            var a = list[0];
            Assert.IsTrue(a.TreeChildren.Count == 2);
            var b = list[1];
            Assert.IsTrue(b.TreeChildren.Count == 1);
        }

        [TestMethod]
        public void SrlzT_Binary_UtilsTeSrlzT_LiteDataTable_Serialization_Binary()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("UserName", typeof(string)));
            table.Columns.Add(new LiteDataColumn("Age", typeof(string)));

            var row = table.NewRow();
            row["UserName"] = "HuQingfang";
            row["Age"] = 26;
            table.Rows.Add(row);

            var row2 = table.NewRow();
            row2["UserName"] = "XuDandan";
            row2["Age"] = 25;
            table.Rows.Add(row2);

            var table2 = ObjectCloner.Clone(table);

            Assert.IsTrue(table2.Rows.Count == 2);
            Assert.IsTrue(table2[0]["UserName"].ToString() == "HuQingfang", "反序列化后，可以通过列名来获取列");
            Assert.IsTrue(table2[0].GetString("UserName") == "HuQingfang");
            Assert.IsTrue(table2[0].GetInt32("Age") == 26);
            Assert.IsTrue(table2[1].GetString("UserName") == "XuDandan");
            Assert.IsTrue(table2[1].GetInt32("Age") == 25);
        }

        #endregion

        #region WCF 序列化

        /// <summary>
        /// 序列化及反序列化
        /// </summary>
        [TestMethod]
        public void SrlzT_WCF()
        {
            var model = new Article
            {
                Code = "Code11",
                CreateDate = DateTime.Today,
            };

            var model2 = CloneByWCFSerializer(model, out string content);

            Assert.IsTrue(content.Contains("Article"));
            Assert.IsTrue(content.Contains("Code11"));

            Assert.IsTrue(model2.Code == "Code11");
            Assert.IsTrue(model2.CreateDate == DateTime.Today);
        }

        /// <summary>
        /// 属性的变更状态，需要支持序列化和反序列化。
        /// </summary>
        [TestMethod]
        public void SrlzT_WCF_MP_ChangedStatus()
        {
            var user = new TestUser();
            user.Name = "1";
            user.MarkPropertiesUnchanged();
            user.Age = 100;

            var fields = ManagedPropertyTest.GetChangedProperties(user);
            Assert.AreEqual(1, fields.Count);
            Assert.AreSame(TestUser.AgeProperty, fields[0].Property);

            var user2 = CloneByWCFSerializer(user);

            fields = ManagedPropertyTest.GetChangedProperties(user2);
            Assert.AreEqual(1, fields.Count);
            Assert.AreSame(TestUser.AgeProperty, fields[0].Property);
        }

        /// <summary>
        /// 被禁用的属性，经过序列化和反序列化后，应该还是禁用状态的。
        /// </summary>
        [TestMethod]
        public void SrlzT_WCF_MP_DisabledStatus()
        {
            var user = new TestUser();
            user.Name = "1";
            user.Age = 100;

            user.Disable(TestUser.NameProperty);

            var user2 = CloneByWCFSerializer(user);

            Assert.IsTrue(user2.IsDisabled(TestUser.NameProperty), "被禁用的属性，反序列化后，也应该是禁用状态。");
        }

        [TestMethod]
        public void SrlzT_WCF_RefId()
        {
            var model = new Article
            {
                UserId = 111,
            };

            var model2 = CloneByWCFSerializer(model, out string content);
            Assert.IsTrue(content.Contains("Article"));
            Assert.IsTrue(content.Contains("111"));

            Assert.IsTrue(model2.UserId == 111);
        }

        [TestMethod]
        public void SrlzT_WCF_Ref()
        {
            var model = new Article
            {
                User = new BlogUser
                {
                    Id = 111,
                    UserName = "HuQingFang"
                }
            };

            var model2 = CloneByWCFSerializer(model, out string content);

            Assert.IsTrue(content.Contains("Article"));
            Assert.IsTrue(content.Contains("<User"));
            Assert.IsTrue(content.Contains("111"));
            Assert.IsTrue(content.Contains("<UserName"));
            Assert.IsTrue(content.Contains("HuQingFang"));

            Assert.IsTrue(model2.UserId == 111);
            Assert.IsTrue(model2.GetProperty(Article.UserProperty) != null);
            Assert.IsTrue(model2.User.UserName == "HuQingFang");
        }

        [TestMethod]
        public void SrlzT_WCF_List()
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

            var model2 = CloneByWCFSerializer(model, out string content);

            Assert.IsTrue(content.Contains("<Book"));
            Assert.IsTrue(content.Contains("<ChapterList"));
            Assert.IsTrue(content.Contains("<Chapter"));
            Assert.IsTrue(content.Contains("111"));
            Assert.IsTrue(content.Contains("<Name"));
            Assert.IsTrue(content.Contains("Chapter1"));

            Assert.IsTrue(model2.GetProperty(Book.ChapterListProperty) != null);
            Assert.IsTrue(model2.ChapterList.Count == 2);
            Assert.IsTrue(model2.ChapterList[0].Id == 111);
            Assert.IsTrue(model2.ChapterList[0].Name == "Chapter1");
        }

        [TestMethod]
        public void zzzSrlzT_WCF_TET()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 序列化及反序列化
        /// </summary>
        [TestMethod]
        public void SrlzT_WCF_LiteDataTable()
        {
            var table = new LiteDataTable();
            table.Columns.Add(new LiteDataColumn("UserName", typeof(string)));
            table.Columns.Add(new LiteDataColumn("Age", typeof(string)));

            var row = table.NewRow();
            row["UserName"] = "HuQingfang";
            row["Age"] = 26;
            table.Rows.Add(row);

            var row2 = table.NewRow();
            row2["UserName"] = "XuDandan";
            row2["Age"] = 25;
            table.Rows.Add(row2);

            //序列化。
            var serializer = new DataContractSerializer(typeof(LiteDataTable));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, table);

            //读取 xml
            byte[] bytes = stream.ToArray();
            string xml = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            Assert.IsTrue(xml.Contains("LiteDataTable"));
            Assert.IsTrue(xml.Contains("HuQingfang"));

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var table2 = (LiteDataTable)serializer.ReadObject(stream);

            Assert.IsTrue(table2.Rows.Count == 2);
            Assert.IsTrue(table2[0]["UserName"].ToString() == "HuQingfang", "反序列化后，可以通过列名来获取列");
            Assert.IsTrue(table2[0].GetString("UserName") == "HuQingfang");
            Assert.IsTrue(table2[0].GetInt32("Age") == 26);
            Assert.IsTrue(table2[1].GetString("UserName") == "XuDandan");
            Assert.IsTrue(table2[1].GetInt32("Age") == 25);
        }

        private static T CloneByWCFSerializer<T>(T entity)
            where T: Entity
        {
            return CloneByWCFSerializer(entity, out string xml);
        }

        private static T CloneByWCFSerializer<T>(T entity, out string content)
            where T: Entity
        {
            //序列化。
            var serializer = CreateWCFSerializer(entity);
            var stream = new MemoryStream();
            serializer.WriteObject(stream, entity);

            byte[] bytes = stream.ToArray();
            content = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            //反序列化
            stream.Seek(0, SeekOrigin.Begin);
            var entity2 = serializer.ReadObject(stream);

            return (T)entity2;
        }

        private static XmlObjectSerializer CreateWCFSerializer(Entity entity)
        {
#if NET45
            //IWcfPortal 上标记了，使用 NetDataContractSerializer 来进行序列化和反序列化。
            //详见 UseNetDataContractAttribute 的类型注释。
            return new NetDataContractSerializer();
#endif
#if NS2
            return SerializationEntityGraph.CreateSerializer(entity.GetRepository().EntityMeta);
#endif
        }

        #endregion

        #region AggtSerializer Web 序列化

        [TestMethod]
        public void SrlzT_Json_MPT_String()
        {
            var e1 = new TestUser();
            e1.Age = 15;
            e1._mySelfReference = e1;
            TestUserExt.SetUserCode(e1, "TestUserExt_UserCode");

            Assert.AreEqual(e1.Validate().Count, 1);

            //在这里可以查看序列化后传输的字符串
            var serializedString = MobileObjectFormatter.SerializeToString(e1);
            Assert.IsNotNull(serializedString);
            var serializedXml = MobileObjectFormatter.SerializeToXml(e1);
            Assert.IsNotNull(serializedXml);

            Assert.IsTrue(serializedXml.Contains("Age"));
            Assert.IsTrue(serializedXml.Contains("UserCode"));
        }

        [TestMethod]
        public void SrlzT_Web()
        {
            var entity = new Favorate
            {
                Name = "name"
            };
            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""name"": ""name""
}");
        }

        /// <summary>
        /// 序列化时，如果属性处于禁用状态，则不需要进行序列化。
        /// </summary>
        [TestMethod]
        public void SrlzT_Web_IgnoreDisabledStatusProperties()
        {
            var entity = new Favorate
            {
                FavorateType = FavorateType.B,
                Name = "name"
            };
            entity.Disable(Favorate.FavorateTypeProperty);

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""name"": ""name""
}");
        }

        [TestMethod]
        public void SrlzT_Web_Enum()
        {
            var entity = new Favorate
            {
                FavorateType = FavorateType.B
            };
            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.EnumSerializationMode = EnumSerializationMode.Integer;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""favorateType"": 1
}");
        }

        [TestMethod]
        public void SrlzT_Web_EnumString()
        {
            var entity = new Favorate
            {
                FavorateType = FavorateType.B
            };
            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.EnumSerializationMode = EnumSerializationMode.String;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""favorateType"": ""B""
}");
        }

        [TestMethod]
        public void SrlzT_Web_EnumWithLabel()
        {
            var entity = new Favorate
            {
                FavorateTypeWithLabel = FavorateTypeWithLabel.B
            };
            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.EnumSerializationMode = EnumSerializationMode.EnumLabel;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""favorateTypeWithLabel"": ""第二个""
}");
        }

        [TestMethod]
        public void SrlzT_Web_Aggt()
        {
            var entity = new Book
            {
                Name = "book",
                ChapterList =
                {
                    new Chapter
                    {
                        Name = "chapter1"
                    },
                    new Chapter
                    {
                        Name = "chapter2",
                        SectionList =
                        {
                            new Section
                            {
                                Name = "section"
                            }
                        }
                    },
                }
            };
            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);
            Assert.AreEqual(json,
    @"{
  ""chapterList"": [
    {
      ""name"": ""chapter1""
    },
    {
      ""name"": ""chapter2"",
      ""sectionList"": [
        {
          ""name"": ""section""
        }
      ]
    }
  ],
  ""name"": ""book""
}");
        }

        [TestMethod]
        public void SrlzT_Web_Ref()
        {
            var entity = new Favorate
            {
                Name = "name",
                Book = new Book { Id = 100, Name = "book" }
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);

            Assert.AreEqual(json,
    @"{
  ""book"": {
    ""id"": 100,
    ""name"": ""book""
  },
  ""bookId"": 100,
  ""name"": ""name""
}");
        }

        [TestMethod]
        public void SrlzT_Web_EntityList()
        {
            var list = new FavorateList
            {
                new Favorate { Name = "f1" },
                new Favorate { Name = "f2" },
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(list);

            Assert.AreEqual(json,
    @"[
  {
    ""name"": ""f1""
  },
  {
    ""name"": ""f2""
  }
]");
        }

        [TestMethod]
        public void SrlzT_Web_OutputListTotalCount()
        {
            var list = new BookList
            {
                new Book { Name = "book1" },
                new Book { Name = "book2" },
            };
            list.SetTotalCount(1000);

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;

            serializer.OutputListTotalCount = true;

            var json = serializer.Serialize(list);

            Assert.AreEqual(json,
    @"{
  ""totalCount"": 1000,
  ""data"": [
    {
      ""name"": ""book1""
    },
    {
      ""name"": ""book2""
    }
  ]
}");
        }

        [TestMethod]
        public void SrlzT_Web_OutputListTotalCount_Aggt()
        {
            var list = new BookList
            {
                new Book
                {
                    Name = "book",
                    ChapterList =
                    {
                        new Chapter
                        {
                            Name = "chapter1"
                        },
                        new Chapter
                        {
                            Name = "chapter2",
                            SectionList =
                            {
                                new Section
                                {
                                    Name = "section"
                                }
                            }
                        },
                    }
                }
            };
            list.SetTotalCount(1000);

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.OutputListTotalCount = true;

            var json = serializer.Serialize(list);

            Assert.AreEqual(json,
    @"{
  ""totalCount"": 1000,
  ""data"": [
    {
      ""chapterList"": [
        {
          ""name"": ""chapter1""
        },
        {
          ""name"": ""chapter2"",
          ""sectionList"": [
            {
              ""name"": ""section""
            }
          ]
        }
      ],
      ""name"": ""book""
    }
  ]
}");
        }

        [TestMethod]
        public void SrlzT_Web_NoCamel()
        {
            var entity = new Favorate
            {
                Name = "name"
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.UseCamelProperty = false;
            var json = serializer.Serialize(entity);

            Assert.AreEqual(json,
    @"{
  ""Name"": ""name""
}");
        }

        [TestMethod]
        public void SrlzT_Web_IgnoreDefault()
        {
            var entity = new Favorate
            {
                Name = "name"
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = false;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);

            Assert.AreEqual(json,
    @"{
  ""createdTime"": ""2000-01-01T00:00:00"",
  ""createdUser"": """",
  ""id"": 0,
  ""updatedTime"": ""2000-01-01T00:00:00"",
  ""updatedUser"": """",
  ""arrayValue"": null,
  ""bookId"": 0,
  ""bytesContent"": """",
  ""favorateType"": 0,
  ""favorateTypeWithLabel"": 0,
  ""listValue"": null,
  ""name"": ""name"",
  ""nullableFavorateType"": null
}");
        }

        [TestMethod]
        public void SrlzT_Web_ArrayValue()
        {
            var entity = new Favorate
            {
                ListValue = new List<string> { "1", "2", "3" },
                ArrayValue = new int[] { 1, 2, 3 },
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);

            Assert.AreEqual(json,
    @"{
  ""arrayValue"": [
    1,
    2,
    3
  ],
  ""listValue"": [
    ""1"",
    ""2"",
    ""3""
  ]
}");
        }

        [TestMethod]
        public void SrlzT_Web_Bytes()
        {
            var entity = new Favorate
            {
                BytesContent = Encoding.UTF8.GetBytes("test content")
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(entity);

            Assert.AreEqual(json,
    @"{
  ""bytesContent"": ""dGVzdCBjb250ZW50""
}");
        }

        [TestMethod]
        public void SrlzT_Web_TreeEntity()
        {
            var list = new FolderList
            {
                new Folder
                {
                    TreeChildren =
                    {
                        new Folder
                        {
                            TreeChildren =
                            {
                                new Folder(),
                                new Folder(),
                            }
                        },
                        new Folder(),
                    }
                }
            };

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            var json = serializer.Serialize(list);

            Assert.AreEqual(json,
    @"[
  {
    ""treeIndex"": ""001."",
    ""treeChildren"": [
      {
        ""treeIndex"": ""001.001."",
        ""treePId"": 0,
        ""treeChildren"": [
          {
            ""treeIndex"": ""001.001.001."",
            ""treePId"": 0,
            ""treeChildren"": []
          },
          {
            ""treeIndex"": ""001.001.002."",
            ""treePId"": 0,
            ""treeChildren"": []
          }
        ]
      },
      {
        ""treeIndex"": ""001.002."",
        ""treePId"": 0,
        ""treeChildren"": []
      }
    ]
  }
]");
        }

        [TestMethod]
        public void SrlzT_Web_DynamicProperty()
        {
            var now = new DateTime(2016, 5, 25, 1, 1, 1);

            var entity = new Favorate();
            entity.SetDynamicProperty("dp1", "Value1");
            entity.SetDynamicProperty("dp2", 1);
            entity.SetDynamicProperty("dp3", now);
            entity.SetDynamicProperty("dp4", FavorateTypeWithLabel.B);

            var serializer = new AggtSerializer();
            serializer.Indent = true;
            serializer.IgnoreDefault = true;
            serializer.IgnoreROProperties = true;
            serializer.EnumSerializationMode = EnumSerializationMode.EnumLabel;

            var json = serializer.Serialize(entity);

            Assert.AreEqual(
    @"{
  ""dp1"": ""Value1"",
  ""dp2"": 1,
  ""dp3"": ""2016-05-25T01:01:01"",
  ""dp4"": ""第二个""
}", json);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization()
        {
            var json =
@"{
    name : 'name'
}";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
            Assert.AreEqual(entity.Name, "name");
        }

        //[TestMethod]
        //public void SrlzT_Web_Deserialization_DisabledPropertyStatus()
        //{
        //    var json = @"{}";
        //    var deserializer = new AggtDeserializer();
        //    var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
        //    Assert.IsTrue(entity.IsDisabled(Favorate.NameProperty), "未给出值的属性，都应该是禁用的。");
        //    Assert.IsTrue(entity.IsDisabled(Favorate.FavorateTypeProperty), "未给出值的属性，都应该是禁用的。");
        //}

        [TestMethod]
        public void SrlzT_Web_Deserialization_Update_CreateNewInstance()
        {
            var json = @"{
""id"": 1,
""name"": ""n2""
}";

            var deserializer = new AggtDeserializer();
            deserializer.UpdatedEntityCreationMode = UpdatedEntityCreationMode.CreateNewInstance;
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

            Assert.AreEqual(1, entity.Id);
            Assert.AreEqual(entity.Name, "n2");
            Assert.AreEqual(entity.FavorateType, FavorateType.A, "CreateNewInstance 导致原有的值丢失。");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Update_RequeryFromRepository()
        {
            var repo = RF.ResolveInstance<FavorateRepository>();
            using (RF.TransactionScope(repo))
            {
                var f1 = new Favorate();
                f1.Name = "n1";
                f1.FavorateType = FavorateType.B;
                repo.Save(f1);

                var json = @"{
""id"": " + f1.Id + @",
""name"": ""n2""
}";

                var deserializer = new AggtDeserializer();
                deserializer.UpdatedEntityCreationMode = UpdatedEntityCreationMode.RequeryFromRepository;
                var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

                Assert.AreEqual(f1.Id, entity.Id);
                Assert.AreEqual(entity.Name, "n2");
                Assert.AreEqual(entity.FavorateType, FavorateType.B, "RequeryFromRepository 导致原有的值不会丢失。");
            }
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_ROProperty()
        {
            var json = @"{
""favorateType"": ""B"",
""RO_FavorateTypeString"": ""XXXXXXXXXXXX""
}";

            var deserializer = new AggtDeserializer();
            deserializer.UpdatedEntityCreationMode = UpdatedEntityCreationMode.CreateNewInstance;
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

            Assert.AreEqual(entity.FavorateType, FavorateType.B);
            Assert.AreEqual(entity.RO_FavorateTypeString, "B", "只读属性的反序列化需要被忽略。");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_RedundancyProperty1()
        {
            var repoA = RF.ResolveInstance<ARepository>();
            var repo = RF.ResolveInstance<BRepository>();
            using (RF.TransactionScope(repo))
            {
                var a = new A { Name = "a" };
                repoA.Save(a);

                var json = @"{
""AName"": ""b"",
""AId"": " + a.Id + @"
}";

                var deserializer = new AggtDeserializer();
                var entity = deserializer.Deserialize(typeof(B), json) as B;

                Assert.AreEqual(entity.AName, "a", "冗余属性需要支持反序列化。同时，当反序列化的值是错的时候，应该以引用属性的值为主。");
            }
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_RedundancyProperty2()
        {
            var repoA = RF.ResolveInstance<ARepository>();
            var repo = RF.ResolveInstance<BRepository>();
            using (RF.TransactionScope(repo))
            {
                var a = new A { Name = "a" };
                repoA.Save(a);

                var json = @"{
""AId"": " + a.Id + @",
""AName"": ""b""
}
            ";

                var deserializer = new AggtDeserializer();
                var entity = deserializer.Deserialize(typeof(B), json) as B;

                Assert.AreEqual(entity.AName, "a", "冗余属性需要支持反序列化。同时，当反序列化的值是错的时候，应该以引用属性的值为主。");
            }
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Enum()
        {
            var json =
@"{
""favorateType"": 1
}";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
            Assert.AreEqual(entity.FavorateType, FavorateType.B);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_EnumString()
        {
            var json =
@"{
""favorateType"": ""B""
}";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
            Assert.AreEqual(entity.FavorateType, FavorateType.B);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_EnumWithLabel()
        {
            var json =
@"{
  ""favorateTypeWithLabel"": ""第二个""
}";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
            Assert.AreEqual(entity.FavorateTypeWithLabel, FavorateTypeWithLabel.B);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Ref()
        {
            var json =
@"{
    name : 'name',
    book : {
        id : 100,
        name: 'book'
    }
}";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

            Assert.AreEqual(entity.Name, "name");
            Assert.IsNull(entity.BookId);
            Assert.IsNull(entity.Book, "一般引用属性不支持反序列化。");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Aggt()
        {
            var json = @"{
  ""chapterList"": [
    {
      ""name"": ""chapter1""
    },
    {
      ""name"": ""chapter2"",
      ""sectionList"": [
        {
          ""name"": ""section""
        }
      ]
    }
  ],
  ""name"": ""book""
}";

            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Book), json) as Book;

            Assert.AreEqual(entity.Name, "book");
            Assert.AreEqual(entity.ChapterList.Count, 2);
            Assert.AreEqual(entity.ChapterList[0].Name, "chapter1");
            Assert.AreEqual(entity.ChapterList[1].Name, "chapter2");
            Assert.AreEqual(entity.ChapterList[1].SectionList.Count, 1);
            Assert.AreEqual(entity.ChapterList[1].SectionList[0].Name, "section");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_EntityList()
        {
            var json = @"[
  {
    ""name"": ""f1""
  },
  {
    ""name"": ""f2""
  }
]";

            var deserializer = new AggtDeserializer();
            var list = deserializer.Deserialize(typeof(FavorateList), json) as FavorateList;

            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(list[0].Name, "f1");
            Assert.AreEqual(list[1].Name, "f2");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Status()
        {
            var repo = RF.ResolveInstance<FavorateRepository>();
            using (RF.TransactionScope(repo))
            {
                var f1 = new Favorate();
                f1.Name = "n1";
                repo.Save(f1);
                var f2 = new Favorate();
                repo.Save(f2);

                var json = @"[
  {
    ""persistenceStatus"": ""new""
  },
  {
  },
  {
    ""id"": " + f2.Id + @",
    ""persistenceStatus"": ""Deleted""
  },
  {
    ""id"": " + f1.Id + @",
    ""name"": ""n2""
  },
  {
    ""persistenceStatus"": ""modified""
  }
]";

                var deserializer = new AggtDeserializer();
                var list = deserializer.Deserialize(typeof(FavorateList), json) as FavorateList;

                Assert.AreEqual(list.Count, 5);
                Assert.AreEqual(list.Concrete().Count(c => c.PersistenceStatus == PersistenceStatus.New), 2);
                Assert.AreEqual(list.Concrete().Count(c => c.PersistenceStatus == PersistenceStatus.Deleted), 1);
                Assert.AreEqual(list.Concrete().Count(c => c.PersistenceStatus == PersistenceStatus.Modified), 2);
            }
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_ArrayValue()
        {
            var json = @"
{
    arrayValue : [1, 2, 3],
    listValue : [""1"", ""2"", ""3""]
}
";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;
            Assert.AreEqual(entity.ArrayValue.Length, 3);
            Assert.AreEqual(entity.ListValue.Count, 3);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_Bytes()
        {
            var json = @"
{
  ""bytesContent"": ""dGVzdCBjb250ZW50""
}
";
            var deserializer = new AggtDeserializer();
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

            Assert.IsNotNull(entity.BytesContent);
            Assert.AreEqual(Encoding.UTF8.GetString(entity.BytesContent), "test content");
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_TreeEntity()
        {
            var json = @"[
  {
    ""treeIndex"": ""001."",
    ""treeChildren"": [
      {
        ""treeIndex"": ""001.001."",
        ""treePId"": 0,
        ""treeChildren"": [
          {
            ""treeIndex"": ""001.001.001."",
            ""treePId"": 0,
            ""treeChildren"": []
          },
          {
            ""treeIndex"": ""001.001.002."",
            ""treePId"": 0,
            ""treeChildren"": []
          }
        ]
      },
      {
        ""treeIndex"": ""001.002."",
        ""treePId"": 0,
        ""treeChildren"": []
      }
    ]
  }
]";
            var deserializer = new AggtDeserializer();
            var list = deserializer.Deserialize(typeof(FolderList), json) as FolderList;

            var root = list[0];
            Assert.AreEqual(root.TreeIndex, "001.");
            Assert.AreEqual(root.TreeChildren.Count, 2);
            Assert.AreEqual(root.TreeChildren[0].TreeIndex, "001.001.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren.Count, 2);
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[0].TreeIndex, "001.001.001.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[0].TreeChildren.Count, 0);
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[1].TreeIndex, "001.001.002.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[1].TreeChildren.Count, 0);
            Assert.AreEqual(root.TreeChildren[1].TreeIndex, "001.002.");
            Assert.AreEqual(root.TreeChildren[1].TreeChildren.Count, 0);
        }

        [TestMethod]
        public void SrlzT_Web_Deserialization_DynamicProperty()
        {
            var json =
@"{
  ""dp1"": ""Value1"",
  ""dp2"": 1,
  ""dp3"": ""2016-05-25T01:01:01"",
  ""dp4"": ""2016-05-25 1:01:01"",
  ""dp5"": ""第二个""
}";

            var deserializer = new AggtDeserializer();
            deserializer.UnknownAsDynamicProperties = true;
            var entity = deserializer.Deserialize(typeof(Favorate), json) as Favorate;

            Assert.IsNotNull(entity);
            Assert.AreEqual(5, entity.DynamicPropertiesCount);
            Assert.AreEqual("Value1", entity.GetDynamicProperty("dp1"));
            Assert.AreEqual(1L, entity.GetDynamicProperty("dp2"));
            Assert.AreEqual(new DateTime(2016, 5, 25, 1, 1, 1), entity.GetDynamicProperty("dp3"));
            Assert.AreEqual("2016-05-25 1:01:01", entity.GetDynamicProperty("dp4"));
            Assert.AreEqual(FavorateTypeWithLabel.B, entity.GetDynamicPropertyOrDefault("dp5", FavorateTypeWithLabel.A));
        }

        [TestMethod]
        public void zzzSrlzT_Web_TET()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
