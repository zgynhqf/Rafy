using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OEA;
using OEA.Library;
using OEA.Library._Test;
using OEA.Reflection;
using OEA.ORM.DbMigration;

namespace OEAUnitTest
{
    [TestClass]
    public class TreeEntityTest : TestBase
    {
        [ClassInitialize]
        public static void TreeEntityTest_ClassInitialize(TestContext context)
        {
            ClassInitialize(context);

            using (var db = new OEADbMigrationContext(UnitTestEntity.ConnectionString))
            {
                db.AutoMigrate();
            }
        }

        /// <summary>
        /// 通过 TreeChildren 修改的关系，同步到 TreeParent 上。
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeRelation_ByChildrenCollection()
        {
            var root = new TestTreeTask
            {
                TreeCode = "1.",
                TreeChildren =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask(),
                            new TestTreeTask(),
                        }
                    },
                    new TestTreeTask(),
                }
            };

            Assert.IsTrue(root.TreeChildren[0].TreeParent == root);
            Assert.IsTrue(root.TreeChildren[1].TreeParent == root);
            var task11 = root.TreeChildren[0];
            Assert.IsTrue(task11.TreeChildren[0].TreeParent == task11);
            Assert.IsTrue(task11.TreeChildren[1].TreeParent == task11);
        }

        /// <summary>
        /// 通过 TreeParent 修改的关系，同步到 TreeChildren 上。
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeRelation_ByTreeParent()
        {
            var root = new TestTreeTask
            {
                TreeCode = "1.",
                TreeChildren =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask(),
                            new TestTreeTask(),
                        }
                    },
                    new TestTreeTask(),
                }
            };

            var root2 = new TestTreeTask { TreeCode = "1." };
            root.TreeParent = root2;

            Assert.IsTrue(root2.TreeChildren[0].TreeParent == root2);
            Assert.IsTrue(root.TreeChildren[0].TreeParent == root);
            Assert.IsTrue(root.TreeChildren[1].TreeParent == root);
            var task11 = root.TreeChildren[0];
            Assert.IsTrue(task11.TreeChildren[0].TreeParent == task11);
            Assert.IsTrue(task11.TreeChildren[1].TreeParent == task11);
        }

        /// <summary>
        /// 向列表中添加根对象
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_AutoCode_AddRoot()
        {
            //把所有对象组装好，一次性通过根对象加入到列表中。
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };
            var root = user.TestTreeTaskList[0];

            Assert.AreEqual(root.TreeChildren[0].TreeCode, "1.1.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[0].TreeCode, "1.1.1.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[1].TreeCode, "1.1.2.");
            Assert.AreEqual(root.TreeChildren[1].TreeCode, "1.2.");
        }

        /// <summary>
        /// 向某个对象添加子对象
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_AutoCode_AddChild()
        {
            //先加入一个根对象，然后再加入子对象。
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask()
                }
            };
            var root = user.TestTreeTaskList[0];
            root.TreeChildren.Add(new TestTreeTask
            {
                TreeChildren =
                {
                    new TestTreeTask(),
                    new TestTreeTask(),
                }
            });
            root.TreeChildren.Add(new TestTreeTask());

            Assert.AreEqual(root.TreeChildren[0].TreeCode, "1.1.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[0].TreeCode, "1.1.1.");
            Assert.AreEqual(root.TreeChildren[0].TreeChildren[1].TreeCode, "1.1.2.");
            Assert.AreEqual(root.TreeChildren[1].TreeCode, "1.2.");
        }

        /// <summary>
        /// 统一关闭整个列表的自动编码生成行为
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_AutoCode_DisableAutoCode()
        {
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };

            user.TestTreeTaskList.AutoTreeCodeEnabled = false;
            user.TestTreeTaskList.Add(new TestTreeTask());
            user.TestTreeTaskList.Add(new TestTreeTask());

            var list = user.TestTreeTaskList;
            Assert.AreEqual(list[5].TreeCode, string.Empty);
            Assert.AreEqual(list[6].TreeCode, string.Empty);
        }

        /// <summary>
        /// 在 TreeCildren 集合中添加的项，会直接自动添加到其所在的列表中。
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeList_AddTreeItem()
        {
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };

            var treeList = user.TestTreeTaskList;
            Assert.AreEqual(treeList.Count, 5);
            Assert.AreEqual(treeList[0].TreeCode, "1.");
            Assert.AreEqual(treeList[1].TreeCode, "1.1.");
            Assert.AreEqual(treeList[2].TreeCode, "1.1.1.");
            Assert.AreEqual(treeList[3].TreeCode, "1.1.2.");
            Assert.AreEqual(treeList[4].TreeCode, "1.2.");
        }

        /// <summary>
        /// 在 TreeCildren 集合中删除的项，会直接自动从其所在的列表中删除。
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeList_Remove_Clear()
        {
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };

            var list = user.TestTreeTaskList;
            var root = list[0];

            root.TreeChildren.Clear();
            Assert.IsTrue(list.Count == 1);
        }

        /// <summary>
        /// 在 TreeCildren 集合中删除的项，会直接自动从其所在的列表中删除。
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeList_Remove_RemoveItem()
        {
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };

            var list = user.TestTreeTaskList;
            var root = list[0];

            root.TreeChildren.RemoveAt(0);
            Assert.IsTrue(list.Count == 2);
        }

        /// <summary>
        /// 升级、降级
        /// </summary>
        [TestMethod]
        public void TreeEntityTest_TreeList_ChangeNodeLevel()
        {
            var user = new TestUser
            {
                TestTreeTaskList =
                {
                    new TestTreeTask
                    {
                        TreeChildren =
                        {
                            new TestTreeTask
                            {
                                TreeChildren =
                                {
                                    new TestTreeTask(),
                                    new TestTreeTask(),
                                }
                            },
                            new TestTreeTask(),
                        }
                    }
                }
            };

            var list = user.TestTreeTaskList;
            var root = user.TestTreeTaskList[0];

            //移动
            root.TreeChildren[0].TreeParent = root.TreeChildren[1];

            var treeList = user.TestTreeTaskList;
            Assert.AreEqual(treeList.Count, 5);
            Assert.AreEqual(treeList[0].TreeCode, "1.");
            Assert.AreEqual(treeList[1].TreeCode, "1.1.");
            Assert.AreEqual(treeList[2].TreeCode, "1.1.1.");
            Assert.AreEqual(treeList[3].TreeCode, "1.1.1.1.");
            Assert.AreEqual(treeList[4].TreeCode, "1.1.1.2.");
        }
    }
}