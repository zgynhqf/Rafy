using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Domain;
using UT;
using Rafy.Reflection;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Utils;
using Rafy.Domain.ORM;

namespace RafyUnitTest
{
    [TestClass]
    public class TreeEntityTest
    {
        [ClassInitialize]
        public static void TET_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        #region 新用例说明

        //编写测试用例（具体写设计文档）
        //  结构
        //    关系
        //    编码
        //    懒加载
        //    ITreeComponent
        //  查询
        //  保存
        //    增加
        //    删除
        //    移动
        //    混合
        //  序列化

        #endregion

        #region 保存

        /// <summary>
        /// 保存实体列表，会保存整个树。
        /// 
        /// 查询出的 EntityList 应该只有根节点。
        /// 
        /// 查询出的结构应该正确。
        /// </summary>
        [TestMethod]
        public void TET_Save_ByEntityList()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        Name = "001.",
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    },
                    new Folder{Name = "002."},
                };
                repo.Save(list);

                var list2 = repo.GetAll();
                Assert.IsTrue(list2.Count == 2, "查询出的 EntityList 应该只有根节点。");
                Assert.IsTrue(list2[0].Name == "001.", "查询出的结构应该正确。");
                Assert.IsTrue(list2[0].TreeChildren.Count == 1, "查询出的结构应该正确。");
                Assert.IsTrue(list2[0].TreeChildren[0].TreeIndex == "001.001.", "查询出的结构应该正确。");
                Assert.IsTrue(list2[1].Name == "002.", "查询出的结构应该正确。");
            }
        }

        /// <summary>
        /// 保存某个实体，会保存该子树。
        /// </summary>
        [TestMethod]
        public void TET_Save_ByEntity()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var node = new Folder
                {
                    TreeIndex = "001.",
                    Name = "001.",
                    TreeChildren = 
                    {
                        new Folder
                        {
                            TreeChildren = 
                            {
                                new Folder()
                            }
                        },
                        new Folder()
                    }
                };
                repo.Save(node);

                var node2 = repo.GetAll()[0];
                Assert.IsTrue(node2.Name == "001.");
                Assert.IsTrue(node2.TreeChildren.Count == 2);
                Assert.IsTrue(node2.TreeChildren[0].TreeIndex == "001.001.");
                Assert.IsTrue(node2.TreeChildren[0].TreeChildren[0].TreeIndex == "001.001.001.");
                Assert.IsTrue(node2.TreeChildren[1].TreeIndex == "001.002.");
            }
        }

        /// <summary>
        /// 在 EntityList 中添加的节点，可以被保存。
        /// </summary>
        [TestMethod]
        public void TET_Save_Add_ByEntityList()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                list.Add(new Folder
                {
                    TreeChildren = 
                    {
                        new Folder()
                    }
                });
                repo.Save(list);

                var list2 = repo.GetAll();
                Assert.IsTrue(list2.Count == 2);
                Assert.IsTrue(list2[1].TreeIndex == "002.");
                Assert.IsTrue(list2[1].TreeChildren.Count == 1);
            }
        }

        /// <summary>
        /// 在 TreeChildren 中添加的子树节点，可以被保存。
        /// </summary>
        [TestMethod]
        public void TET_Save_Add_ByTreeChildren()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                var root = list[0];
                root.TreeChildren.Add(new Folder
                {
                    TreeChildren = 
                    {
                        new Folder()
                    }
                });
                repo.Save(root);

                var list2 = repo.GetAll();
                root = list[0];
                Assert.IsTrue(root.TreeChildren.Count == 2);
                Assert.IsTrue(root.TreeChildren[1].TreeIndex == "001.002.");
                Assert.IsTrue(root.TreeChildren[1].TreeChildren.Count == 1);
                Assert.IsTrue(root.TreeChildren[1].TreeChildren[0].TreeIndex == "001.002.001.");
            }
        }

        /// <summary>
        /// 在直接删除一个实体时，不论它的子节点加载没有加载，都应该被删除。
        /// </summary>
        [TestMethod]
        public void TET_Save_Remove_ByEntity_WithUnloadedTreeChildren()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder
                            {
                                TreeChildren = 
                                {
                                    new Folder()
                                }
                            },
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);

                var item = repo.GetById(list[0].Id);
                item.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(item);

                var list2 = repo.GetAll();
                Assert.IsTrue(list2.Count == 0);
            }
        }

        /// <summary>
        /// 在 EntityList 中删除的节点，可以被保存。
        /// </summary>
        [TestMethod]
        public void TET_Save_Remove_ByEntityList()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    },
                    new Folder(),
                });

                var list = repo.GetAll();
                Assert.IsTrue(list.Count == 2);

                list.RemoveAt(0);
                repo.Save(list);

                var list2 = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
            }
        }

        /// <summary>
        /// 在 TreeChildren 中删除的节点，可以被保存。
        /// </summary>
        [TestMethod]
        public void TET_Save_Remove_ByTreeChildren()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                var root = list[0];
                Assert.IsTrue(root.TreeChildren.Count == 1);
                root.TreeChildren.Clear();
                repo.Save(root);

                var list2 = repo.GetAll();
                Assert.IsTrue(root.TreeChildren.Count == 0);
            }
        }

        /// <summary>
        /// 如果一个根节点变为非根节点，那么它应该从 List 中移除，这时保存时，该节点不应该被删除。
        /// </summary>
        [TestMethod]
        public void TET_Save_LevelDownRoot()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder(),
                    new Folder(),
                };
                repo.Save(list);

                list = repo.GetAll();
                var a = list[0];
                var b = list[1];
                b.TreeChildren.Add(a);
                Assert.IsTrue(list.Count == 1);

                repo.Save(list);

                list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                b = list[0];
                Assert.IsTrue(b.TreeChildren.Count == 1);
                Assert.IsTrue(b.TreeChildren[0].Id.Equals(a.Id));
            }
        }

        /// <summary>
        /// 把某个子节点 A，从其父节点 B 下移动到子节点 C 下，可以被保存成功。
        /// </summary>
        [TestMethod]
        public void TET_Save_MoveNode()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder(),
                        }
                    },
                    new Folder()
                });

                var list = repo.GetAll();
                var root1 = list[0];
                var root2 = list[1];
                var a = root1.TreeChildren[0];
                root2.TreeChildren.Add(a);
                repo.Save(list);

                list = repo.GetAll();
                root1 = list[0];
                root2 = list[1];
                Assert.IsTrue(root1.TreeChildren.Count == 0);
                Assert.IsTrue(root2.TreeChildren.Count == 1);
                Assert.IsTrue(root2.TreeChildren[0].Id.Equals(a.Id));
            }
        }

        /// <summary>
        /// A 与 B 是平级节点，先移动 A 到 B 的子节点，然后再删除 B，最后保存。A 应该被删除。
        /// </summary>
        [TestMethod]
        public void TET_Save_Combine_LevelDownAndDelete()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder(),
                    new Folder(),
                    new Folder(),
                });

                var list = repo.GetAll();
                var a = list[0];
                var b = list[1];
                a.TreeParent = b;

                list.Remove(b);
                repo.Save(list);

                list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                var c = list[0];
                Assert.IsTrue(c.TreeChildren.Count == 0);
            }
        }

        /// <summary>
        /// 使用 TreeParent 删除某个带有关系的节点后，再把它加入到树中，保存时，不应该删除该节点。
        /// </summary>
        [TestMethod]
        public void TET_Save_Combine_DeleteByTreeParentAndReAddIt()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                var a = list[0];
                var b = a.TreeChildren[0];
                b.TreeParent = null;

                Assert.IsTrue(a.TreeChildren.DeletedListField != null && a.TreeChildren.DeletedListField.Contains(b));

                list.Add(b);
                list.Remove(a);
                repo.Save(list);

                list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                Assert.IsTrue(list[0].Id.Equals(b.Id),
@"虽然这时 b 在 a 的子节点删除列表中，但是由于后面把 b 又加入到了树中。所以保存后，也不应该把 b 删除。");
            }
        }

        /// <summary>
        /// B 是 A 的子节点，当把 B 升级后，再把 A 删除，应该可以保存成功。
        /// </summary>
        [TestMethod]
        public void TET_Save_Combine_LevelUpAndDelete()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                });

                var list = repo.GetAll();
                var a = list[0];
                var b = a.TreeChildren[0];
                a.TreeChildren.Remove(b);

                list.Add(b);
                list.Remove(a);
                repo.Save(list);

                list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                Assert.IsTrue(list[0].Id.Equals(b.Id));
            }
        }

        /// <summary>
        /// B 是 A 的子节点，先移动 A 使其 TreeIndex 变更，然后升级 B，再删除 A，最后保存。
        /// </summary>
        [TestMethod]
        public void TET_Save_Combine_MoveAndLevelUpAndDelete()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    },
                    new Folder()
                });

                var list = repo.GetAll();
                var a = list[0];
                var b = a.TreeChildren[0];
                var c = list[1];

                a.TreeParent = c;
                b.TreeParent = c;
                c.TreeChildren.Remove(a);
                Assert.IsTrue(list.Count == 1);
                Assert.IsTrue(list.DeletedList.Count == 0);
                Assert.IsTrue(a.TreePId.Equals(c.Id) && a.IsDeleted);
                repo.Save(list);

                list = repo.GetAll();
                Assert.IsTrue(list.Count == 1);
                c = list[0];
                Assert.IsTrue(c.TreeChildren.Count == 1);
                Assert.IsTrue(c.TreeChildren[0].Id.Equals(b.Id));
            }
        }

        //以下这种场景可以不支持直接更新新实体。需要把旧实体查询出来后，再设置实体属性。
        ///// <summary>
        ///// 对已经保存在数据库中的实体，直接设置所有值来更新所有字段。
        ///// 场景：MVC 中直接把实体作为视图模型传入到界面中时，会出现 TreePId 设置时导致 TreeIndex 出错的问题。
        ///// </summary>
        //[TestMethod]
        //public void TET_Save_UpdateEntityBySetProperty()
        //{
        //    var repo = RF.Concrete<FolderRepository>();
        //    using (RF.TransactionScope(repo))
        //    {
        //        var node = new Folder
        //        {
        //            TreeIndex = "001.",
        //            Name = "001.",
        //            TreeChildren = 
        //            {
        //                new Folder
        //                {
        //                    Name = "002."
        //                }
        //            }
        //        };
        //        repo.Save(node);

        //        var root = repo.GetAll()[0];
        //        var oldNode = root.TreeChildren[0] as Folder;

        //        var mvcModel = new Folder();
        //        mvcModel.Id = oldNode.Id;
        //        mvcModel.TreePId = oldNode.TreePId;
        //        mvcModel.Name = "New Name";
        //        mvcModel.PersistenceStatus = PersistenceStatus.Modified;
        //        Assert.AreEqual(mvcModel.TreeIndex, oldNode.TreeIndex);
        //        repo.Save(mvcModel);

        //        var root2 = repo.GetAll()[0];
        //        var newNode = root2.TreeChildren[0] as Folder;
        //        Assert.AreEqual(newNode.Name, "New Name");
        //    }
        //}

        #endregion

        #region 序列化

        /// <summary>
        /// 二进制正反序列化后，数据对比正确。
        /// </summary>
        [TestMethod]
        public void TET_Serialization_ByBit()
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

        /// <summary>
        /// WCF 正反序列化后，数据对比正确。
        /// </summary>
        [TestMethod]
        public void zzzTET_Serialization_ByWCF()
        {
        }

        /// <summary>
        /// Web 正反序列化后，数据对比正确。
        /// </summary>
        [TestMethod]
        public void zzzTET_Serialization_ByWeb()
        {
        }

        #endregion

        #region 查询

        /// <summary>
        /// 查询出整个/部分树。父节点对象应该与子节点的 TreeParent 是同一个对象。
        /// </summary>
        [TestMethod]
        public void TET_Query_TreeParentRelation()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        TreeChildren = 
                        {
                            new Folder()
                        }
                    }
                };
                repo.Save(list);

                list = repo.GetAll();
                var root = list[0];
                var child = root.TreeChildren[0];
                Assert.IsTrue(child.TreeParent == root);
            }
        }

        /// <summary>
        /// LoadAllParent 查询次数正确，数据正确。
        /// </summary>
        [TestMethod]
        public void TET_Query_LoadAllParent()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var veryChild = new Folder();
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
                                    veryChild
                                }
                            }
                        }
                    }
                };
                repo.Save(list);

                veryChild = repo.GetById(veryChild.Id);
                Assert.IsTrue(!veryChild.IsTreeParentLoaded);
                repo.LoadAllTreeParents(veryChild);
                Assert.IsTrue(veryChild.IsTreeParentLoaded);
                Assert.IsTrue(veryChild.TreeParent.IsTreeParentLoaded);
            }
        }

        /// <summary>
        /// GetByTreeParentCode 数据正确、排序正确。
        /// </summary>
        [TestMethod]
        public void TET_Query_GetByTreeParentCode()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        Name = "1",
                        TreeChildren =
                        {
                            new Folder
                            {
                                Name = "1.1",
                                TreeChildren =
                                {
                                    new Folder{Name = "1.1.1"},
                                    new Folder{Name = "1.1.2"},
                                }
                            },
                            new Folder{Name = "1.2"},
                        }
                    }
                };
                repo.Save(list);

                Assert.IsTrue(list.Count == 1);
                var root = list[0];
                Assert.IsTrue(root.TreeChildren.Count == 2);
                var node11 = root.TreeChildren[0];
                Assert.IsTrue(node11.TreeChildren.Count == 2);
                var a = node11.TreeChildren[0] as Folder;
                Assert.IsTrue(a.Name == "1.1.1");
                var b = node11.TreeChildren[1] as Folder;
                Assert.IsTrue(b.Name == "1.1.2");
            }
        }

        [TestMethod]
        public void TET_Query_GetByTreePId()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        Name = "1",
                        TreeChildren =
                        {
                            new Folder
                            {
                                Name = "1.1",
                                TreeChildren =
                                {
                                    new Folder{Name = "1.1.1"},
                                    new Folder{Name = "1.1.2"},
                                }
                            },
                            new Folder{Name = "1.2"},
                        }
                    }
                };
                repo.Save(list);

                var root = list[0];

                list = repo.GetByTreePId(root.Id);
                Assert.IsTrue(list.Count == 2);
                Assert.IsTrue(list[0].Name == "1.1");
                Assert.IsTrue(list[1].Name == "1.2");
                Assert.IsTrue(!list[0].TreeChildren.IsLoaded, "GetByTreePId 方法返回的数据，只有根节点。");
            }
        }

        /// <summary>
        /// 在查询时加载树节点，如果数据中有不属于这棵树的节点，
        /// 或者某些节点的父节点被过滤了，那么不应该加到这个列表中。
        /// </summary>
        [TestMethod]
        public void TET_Query_LoadSubTreeIgnoreOtherNodes()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        Name = "1",
                        TreeChildren =
                        {
                            new Folder
                            {
                                Name = "1.1",
                                TreeChildren =
                                {
                                    new Folder{ Name = "1.1.1" },
                                    new Folder{ Name = "1.1.2" },
                                }
                            },
                            new Folder{Name = "1.2"},
                        }
                    },
                };
                repo.Save(list);

                var root = list[0];

                list = repo.GetForIgnoreTest();
                Assert.IsTrue(list.Count == 1);
                Assert.IsTrue((list as ITreeComponent).CountNodes() == 2, "内存中只有 1 和 1.2 两个节点。");
            }
        }

        /// <summary>
        /// GetAll 查询出来的树中的所有节点的 IsFullLoaded 都是真的。
        /// </summary>
        [TestMethod]
        public void TET_Query_GetAll_FullLoaded()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var tree = repo.GetAll() as ITreeComponent;
                Assert.IsTrue(tree.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Query_DefaultOrderBy_TreeIndex_IPropertyQuery()
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

            var repo = RF.Concrete<TestTreeTaskRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(user);

                var list = repo.GetByParentId(user.Id);
                for (int i = 1, c = list.Count; i < c; i++)
                {
                    var item1 = list[i - 1];
                    var item2 = list[i];
                    Assert.IsTrue(string.Compare(item1.TreeIndex, item2.TreeIndex) == -1, "默认应该按照 TreeIndex 正序排列。");
                }

                //按照 Id 排序的功能应该无效。
                var success = false;
                try
                {
                    list = repo.GetAndOrderByIdDesc();
                    success = true;
                }
                catch { }
                Assert.IsFalse(success, "默认应该按照 TreeIndex 正 序排列。");
            }
        }

        [TestMethod]
        public void TET_Query_DefaultOrderBy_TreeIndex()
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

            var repo = RF.Concrete<TestTreeTaskRepository>();
            using (RF.TransactionScope(repo))
            {
                RF.Save(user);

                var list = repo.GetByParentId(user.Id);
                for (int i = 1, c = list.Count; i < c; i++)
                {
                    var item1 = list[i - 1];
                    var item2 = list[i];
                    Assert.IsTrue(string.Compare(item1.TreeIndex, item2.TreeIndex) == -1, "默认应该按照 TreeIndex 正序排列。");
                }

                //按照 Id 排序的功能应该无效。
                var success = false;
                try
                {
                    list = repo.GetAndOrderByIdDesc2();
                    success = true;
                }
                catch { }
                Assert.IsFalse(success, "默认应该按照 TreeIndex 正序排列。");
            }
        }

        /// <summary>
        /// 使用 EntityContext 后，再次查询树时，应该查出同一棵树。
        /// </summary>
        [TestMethod]
        public void zzzTET_Query_EntityContext()
        {
        }

        #endregion

        #region 结构 - 关系

        /// <summary>
        /// 树的根节点列表中不能添加非根节点。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_CantAddNonRootIntoRootList()
        {
            var list = new FolderList
            {
                new Folder
                {
                    TreeChildren = 
                    {
                        new Folder()
                    }
                },
            };
            var a = list[0];
            var b = a.TreeChildren[0];

            var hasException = false;
            try
            {
                list.Add(b);
            }
            catch (InvalidOperationException ex)
            {
                hasException = ex.Message.Contains("树的根节点列表中不能添加非根节点");
            }

            Assert.IsTrue(hasException);
        }

        /// <summary>
        /// 如果一个根节点变为非根节点，那么它应该从 List 中移除。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_RemoveFromListIfNotRoot()
        {
            var list = new FolderList
            {
                new Folder(),
                new Folder(),
            };
            var a = list[0];
            var b = list[1];
            b.TreeChildren.Add(a);

            Assert.IsTrue(list.Count == 1);
        }

        /// <summary>
        /// 在 TreeChildren 中添加节点，节点相应的 TreeParent、TreePId 都有值。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreeChildren_Add()
        {
            var root = new Folder
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
            };

            Assert.IsTrue(root.TreeChildren[0].TreeParent == root);
            Assert.IsTrue(root.TreeChildren[1].TreeParent == root);
            var task11 = root.TreeChildren[0];
            Assert.IsTrue(task11.TreeChildren[0].TreeParent == task11);
            Assert.IsTrue(task11.TreeChildren[1].TreeParent == task11);
        }

        /// <summary>
        /// 子集合中删除节点 a 后，a 的状态是，TreeParent，TreePId不变，isDeleted 属性为真。
        /// 表示这个节点还属于原来的父节点，但是是被删除的状态。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreeChildren_Remove()
        {
            var child = new Folder();
            var root = new Folder
            {
                Id = 1,
                TreeChildren =
                {
                    child
                }
            };
            root.MarkSaved();

            Assert.IsTrue(child.TreePId.Equals(1));

            root.TreeChildren.Remove(child);

            Assert.IsTrue(child.IsDeleted);
            Assert.IsTrue(child.TreePId == 1);
            Assert.IsTrue(child.TreeParent == root);
        }

        /// <summary>
        /// 设置 B.TreeParent 为某个节点 A，则 A.TreeChildren 中含有 B；B.TreePId == A.Id.
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreeParent_Set()
        {
            var a = new Folder
            {
                Id = 1234
            };
            var b = new Folder();
            b.TreeParent = a;

            Assert.IsTrue(b.IsTreeParentLoaded);
            Assert.IsTrue(b.TreeParent == a);
            Assert.IsTrue(b.TreePId == a.Id);
            Assert.IsTrue(a.TreeChildren.Count == 1);
            Assert.IsTrue(a.TreeChildren[0] == b);
        }

        /// <summary>
        /// 清空 B.TreeParent，则 A.TreeChildren 中不含 B；B.TreePId 为空。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreeParent_SetNull()
        {
            var a = new Folder
            {
                Id = 1234
            };
            var b = new Folder();
            b.TreeParent = a;

            Assert.IsTrue(b.TreeParent == a);

            b.TreeParent = null;
            Assert.IsTrue(b.TreeParent == null);
            Assert.IsTrue(b.TreePId == null);
        }

        /// <summary>
        /// 设置 A.TreePId 为空，则 A.IsTreeParentLoaded 返回 false。同时，获取 A.TreeParent 返回空。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreePId_SetNull()
        {
            var b = new Folder
            {
                Id = 1234
            };
            var a= new Folder();
            a.TreeParent = b;

            a.TreePId = null;

            Assert.IsTrue(a.IsTreeParentLoaded);
            Assert.IsTrue(a.TreeParent == null);
        }

        /// <summary>
        /// 设置 A.TreePId 为 B.Id 时，由于要加载 TreeIndex，所以 A.IsTreeParentLoaded 返回 true。
        /// A.TreeParent 应该返回 B 的同质对象。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreePId_Set()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var b = new Folder();
                repo.Save(b);

                var a  = new Folder();
                a.TreePId = b.Id;

                Assert.IsTrue(a.IsTreeParentLoaded);
                Assert.IsTrue(a.TreeParent != b && a.TreeParent.Id.Equals(b.Id));
            }
        }

        /// <summary>
        /// 子节点 A 添加到一个新的父节点时，A 的 TreePId 也应该从 null 变为 0。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Relation_TreePId_DefaultValue()
        {
            var child = new Folder();
            Assert.IsTrue(child.TreePId == null);

            new Folder().TreeChildren.Add(child);

            Assert.IsTrue(child.TreePId == 0);
        }

        #endregion

        #region 结构 - 索引 TreeIndex

        /// <summary>
        /// 在 EntityList 中添加节点，节点生成相应的 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_EntityList_RootListAdd()
        {
            var list = new FolderList();

            var a = new Folder();
            list.Add(a);
            Assert.IsTrue(a.TreeIndex == "001.");

            var b = new Folder();
            list.Add(b);
            Assert.IsTrue(b.TreeIndex == "002.");
        }

        /// <summary>
        /// 在非根节点的 EntityList 中添加节点，不会修改添加节点的 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_EntityList_LeafListAdd()
        {
            var a = new Folder
            {
                TreeChildren =
                {
                    new Folder(),
                    new Folder(),
                }
            };
            new FolderList().Add(a);

            var a11 = a.TreeChildren[0];
            Assert.IsTrue(a11.TreeIndex == "001.001.");
            var a12 = a.TreeChildren[1];
            Assert.IsTrue(a12.TreeIndex == "001.002.");

            var sundryList = new FolderList();
            sundryList.Add(a11);
            Assert.IsTrue(a11.TreeIndex == "001.001.");
            sundryList.Add(a12);
            Assert.IsTrue(a12.TreeIndex == "001.002.");
        }

        /// <summary>
        /// 在 EntityList 中某位置插入节点，节点之后的所有节点的 TreeIndex 变更。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_EntityList_Insert()
        {
            var list = new FolderList
            {
                new Folder(),
            };

            var a = list[0];
            Assert.IsTrue(a.TreeIndex == "001.");

            var b = new Folder();
            list.Insert(0, b);
            Assert.IsTrue(b.TreeIndex == "001.");
            Assert.IsTrue(a.TreeIndex == "002.");
        }

        /// <summary>
        /// 在 EntityList 中删除节点，节点之后的所有节点的 TreeIndex 变更。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_EntityList_Remove()
        {
            var list = new FolderList
            {
                new Folder(),
                new Folder(),
            };

            var a = list[1];
            Assert.IsTrue(a.TreeIndex == "002.");

            list.RemoveAt(0);
            Assert.IsTrue(a.TreeIndex == "001.");
        }

        /// <summary>
        /// 在 TreeChildren 中添加节点，节点生成相应的 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreeChildren_Add()
        {
            var a = new Folder();
            var list = new FolderList { a };
            Assert.IsTrue(a.TreeIndex == "001.");

            var a1 = new Folder();
            a.TreeChildren.Add(a1);
            Assert.IsTrue(a1.TreeIndex == "001.001.");

            var a2 = new Folder();
            a.TreeChildren.Add(a2);
            Assert.IsTrue(a2.TreeIndex == "001.002.");

            var a11 = new Folder();
            a2.TreeChildren.Add(a11);
            Assert.IsTrue(a11.TreeIndex == "001.002.001.");
        }

        /// <summary>
        /// 使用集合初始化器进行添加。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreeChildren_Add2()
        {
            var list = new FolderList
            {
                new Folder
                {
                    TreeChildren= 
                    {
                        new Folder
                        {
                            TreeChildren= 
                            {
                                new Folder(),
                            }
                        },
                        new Folder(),
                    }
                }
            };
            var a = list[0];
            Assert.IsTrue(a.TreeIndex == "001.");
            Assert.IsTrue(a.TreeChildren.Count == 2);

            var a1 = a.TreeChildren[0];
            Assert.IsTrue(a1.TreeIndex == "001.001.");
            Assert.IsTrue(a1.TreeChildren.Count == 1);
            Assert.IsTrue(a1.TreeChildren[0].TreeIndex == "001.001.001.");

            var a2 = a.TreeChildren[1];
            Assert.IsTrue(a2.TreeIndex == "001.002.");
        }

        /// <summary>
        /// 在 TreeChildren 中某位置插入节点，节点之后的所有节点的 TreeIndex 变更。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreeChildren_Insert()
        {
            var a = new Folder();
            var list = new FolderList { a };
            Assert.IsTrue(a.TreeIndex == "001.");

            var a1 = new Folder();
            a.TreeChildren.Add(a1);
            Assert.IsTrue(a1.TreeIndex == "001.001.");

            var a2 = new Folder();
            a.TreeChildren.Add(a2);
            Assert.IsTrue(a2.TreeIndex == "001.002.");

            var a11 = new Folder();
            a2.TreeChildren.Add(a11);
            Assert.IsTrue(a11.TreeIndex == "001.002.001.");
        }

        /// <summary>
        /// 在 TreeChildren 中删除节点，节点之后的所有节点的 TreeIndex 变更。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreeChildren_Remove()
        {
            var list = new FolderList
            {
                new Folder
                {
                    TreeChildren= 
                    {
                        new Folder(),
                        new Folder(),
                    }
                }
            };
            var root = list[0];

            root.TreeChildren.RemoveAt(0);
            var a1 = root.TreeChildren[0];
            Assert.IsTrue(a1.TreeIndex == "001.001.");
        }

        /// <summary>
        /// 通过 TreeParent 设置上关系后，节点的 TreeIndex 设置正确。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreeParent_Set()
        {
            var root = new Folder();
            new FolderList().Add(root);
            var child = new Folder();
            child.TreeParent = root;
            Assert.IsTrue(child.TreeIndex == "001.001.");
        }

        /// <summary>
        /// 通过 TreePId 设置上关系后，节点的 TreeIndex 设置正确。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_TreePId_Set()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var root = new Folder();
                new FolderList().Add(root);

                repo.Save(root);

                var child = new Folder();
                child.TreePId = root.Id;
                Assert.IsTrue(child.TreeIndex == "001.001.");
            }
        }

        /// <summary>
        /// 默认最大可以索引到 100.
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_DefaultMaxNumber()
        {
            var root = new Folder();
            new FolderList().Add(root);

            for (int i = 0; i < 10010; i++)
            {
                root.TreeChildren.Add(new Folder());
            }

            Assert.AreEqual(root.TreeChildren[0].TreeIndex, "001.001.");
            Assert.AreEqual(root.TreeChildren[8].TreeIndex, "001.009.");
            Assert.AreEqual(root.TreeChildren[9].TreeIndex, "001.010.");
            Assert.AreEqual(root.TreeChildren[999].TreeIndex, "001.999.");
            Assert.AreEqual(root.TreeChildren[1000].TreeIndex, "001.999.");
        }

        /// <summary>
        /// 可以使用 ResetAllTreeIndex 来重设所有数据行的 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_ResetAllTreeIndex()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder
                    {
                        TreeChildren= 
                        {
                            new Folder
                            {
                                TreeChildren= 
                                {
                                    new Folder(),
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                TreeIndexHelper.ResetTreeIndex(repo);

                var tree = repo.GetAll();
                Assert.IsTrue(tree.Count == 1);
                Assert.IsTrue((tree as ITreeComponent).IsFullLoaded);
                var root = tree[0];
                Assert.IsTrue(root.TreeIndex == "001.");
                Assert.IsTrue(root.TreeChildren.Count == 2);
                Assert.IsTrue(root.TreeChildren[0].TreeIndex == "001.001.");
                Assert.IsTrue(root.TreeChildren[0].TreeChildren.Count == 1);
                Assert.IsTrue(root.TreeChildren[0].TreeChildren[0].TreeIndex == "001.001.001.");
                Assert.IsTrue(root.TreeChildren[1].TreeIndex == "001.002.");
            }
        }

        /// <summary>
        /// 直接添加根节点时，也应该自动生成 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_InsertRootItem()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder()
                };
                repo.Save(list);

                var root2 = new Folder();
                repo.Save(root2);
                Assert.IsTrue(root2.TreeIndex == "002.");

                var tree = repo.GetAll();
                Assert.IsTrue(tree.Count == 2);
                Assert.IsTrue(tree[1].TreeIndex == "002.");
            }
        }

        /// <summary>
        /// 当根节点出现断码时，直接添加根节点时，也应该自动生成不重复的 TreeIndex。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_InsertRootItem_WhileIndexCollapsed()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var list = new FolderList
                {
                    new Folder(),//001.
                    new Folder//002.
                    {
                        TreeChildren = 
                        {
                            new Folder()//002.001.
                        }
                    },
                    new Folder(),//003.
                };
                repo.Save(list);

                //删除 001
                var root1 = list[0];
                root1.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(root1);

                var root2 = new Folder();
                repo.Save(root2);
                Assert.IsTrue(root2.TreeIndex == "004.");

                var tree = repo.GetAll();
                Assert.IsTrue(tree.Count == 3);
                Assert.IsTrue(tree[0].TreeIndex == "002.");
                Assert.IsTrue(tree[0].TreeChildren[0].TreeIndex == "002.001.");
                Assert.IsTrue(tree[1].TreeIndex == "003.");
                Assert.IsTrue(tree[2].TreeIndex == "004.");
            }
        }

        /// <summary>
        /// 统一关闭整个列表的自动编码生成行为
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeIndex_DisableAutoIndex()
        {
            var list = new FolderList();
            list.Add(new Folder());

            list.AutoTreeIndexEnabled = false;

            list.Add(new Folder());

            Assert.AreEqual(list[0].TreeIndex, "001.");
            Assert.AreEqual(list[1].TreeIndex, string.Empty);
        }

        #endregion

        #region 结构 - 懒加载

        /// <summary>
        /// 调用 A.TreeChildren.Load(false)，只加载当前集合中的数据，
        /// A.TreeChildren.IsLoaded 为真，IsFullLoaded 为假，
        /// 而集合中每一个子节点的 TreeChildren.IsLoaded 均为假。
        /// </summary>
        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_Load()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);
                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.Load();
                Assert.IsTrue(root.TreeChildren.IsLoaded);
                foreach (var child in root.TreeChildren)
                {
                    Assert.IsTrue(!child.TreeChildren.IsLoaded);
                }
            }
        }

        /// <summary>
        /// 调用 TreeChildren.Load(true)，其中的所有集合的 IsLoaded 均为真。 
        /// </summary>
        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_Load_Full()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);
                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.LoadAllNodes();
                Assert.IsTrue(root.TreeChildren.IsLoaded);
                foreach (var child in root.TreeChildren)
                {
                    Assert.IsTrue(child.TreeChildren.IsLoaded);
                }
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_Add()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.Add(new Folder());
                Assert.IsTrue(root.TreeChildren.IsLoaded && !root.TreeChildren.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_Insert()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.Insert(0, new Folder());
                Assert.IsTrue(root.TreeChildren.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_Remove()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.RemoveAt(0);
                Assert.IsTrue(root.TreeChildren.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_Clear()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren.Clear();
                Assert.IsTrue(root.TreeChildren.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_GetItem()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                var child = root.TreeChildren[0];
                Assert.IsTrue(root.TreeChildren.IsLoaded && !root.TreeChildren.IsFullLoaded);
            }
        }

        [TestMethod]
        public void TET_Struc_LazyLoad_TreeChildren_AutoLoad_SetItem()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    }
                };
                repo.Save(list);

                var root = repo.GetById(list[0].Id);

                Assert.IsTrue(!root.TreeChildren.IsLoaded);
                root.TreeChildren[0] = new Folder();
                Assert.IsTrue(root.TreeChildren.IsFullLoaded);
            }
        }

        /// <summary>
        /// 从数据库中查询出来的一个子节点 A，调用 A.TreeParent 属性，引发懒加载。
        /// </summary>
        [TestMethod]
        public void TET_Struc_LazyLoad_TreeParent()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
            {
                var veryChild = new Folder();
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
                                    veryChild,
                                }
                            }
                        }
                    }
                };
                repo.Save(list);

                veryChild = repo.GetById(veryChild.Id);

                Assert.IsTrue(!veryChild.IsTreeParentLoaded);
                var p = veryChild.TreeParent;
                Assert.IsTrue(veryChild.IsTreeParentLoaded);
                Assert.IsTrue(p != null);

                Assert.IsTrue(!p.IsTreeParentLoaded);
                var p2 = p.TreeParent;
                Assert.IsTrue(p.IsTreeParentLoaded);
                Assert.IsTrue(p2 != null);
            }
        }

        #endregion

        #region 结构 - ITreeComponent

        /// <summary>
        /// 刚创建的三种 TreeComponent，默认值要正确。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeComponent_DefaultValue_EntityList()
        {
            ITreeComponent component = new FolderList();
            Assert.IsTrue(component.ComponentType == TreeComponentType.NodeList);
            Assert.IsTrue(component.CountNodes() == 0);
            Assert.IsTrue(component.IsFullLoaded);
            Assert.IsTrue(component.TreeComponentParent == null);
        }

        [TestMethod]
        public void TET_Struc_TreeComponent_DefaultValue_Entity()
        {
            ITreeComponent component = new Folder();
            Assert.IsTrue(component.ComponentType == TreeComponentType.Node);
            Assert.IsTrue(component.CountNodes() == 1);
            Assert.IsTrue(component.IsFullLoaded);
            Assert.IsTrue(component.TreeComponentParent == null);
        }

        [TestMethod]
        public void TET_Struc_TreeComponent_DefaultValue_TreeChildren()
        {
            var node = new Folder();
            ITreeComponent component = node.TreeChildren;
            Assert.IsTrue(component.ComponentType == TreeComponentType.TreeChildren);
            Assert.IsTrue(component.CountNodes() == 0);
            Assert.IsTrue(component.IsFullLoaded);
            Assert.IsTrue(component.TreeComponentParent == node);
        }

        [TestMethod]
        public void TET_Struc_TreeComponent_EntityList()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    },
                    new Folder(),
                };
                repo.Save(list);

                var roots = repo.GetTreeRoots() as ITreeComponent;
                TestTreeComponent(roots, 2, 5);
            }
        }

        [TestMethod]
        public void TET_Struc_TreeComponent_Entity()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                }
                            },
                            new Folder(),
                        }
                    },
                };
                repo.Save(list);

                var roots = repo.GetTreeRoots()[0] as ITreeComponent;
                TestTreeComponent(roots, 1, 4);
            }
        }

        [TestMethod]
        public void TET_Struc_TreeComponent_TreeChildren()
        {
            var repo = RF.Concrete<FolderRepository>();
            using (RF.TransactionScope(repo))
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
                                    new Folder
                                    {
                                        TreeChildren = 
                                        {
                                            new Folder(),
                                        }
                                    },
                                    new Folder(),
                                }
                            },
                            new Folder(),
                        }
                    },
                };
                repo.Save(list);

                var treeChildren= repo.GetTreeRoots()[0].TreeChildren;
                Assert.IsTrue(!treeChildren.IsLoaded);
                treeChildren.Load();
                var component = treeChildren as ITreeComponent;
                TestTreeComponent(component, 2, 5);
            }
        }

        private static void TestTreeComponent(ITreeComponent component, int countBefore, int countAfter)
        {
            //只查询一级节点，统计该节点的个数应该正确。同时，它的 IsFullLoaded 返回假。
            //此时，调用 EachNode 遍历出的个数，应该正确。
            //调用 LoadAllNodes 查询整个树后，统计该节点的个数应该正确。
            //此时，调用 EachNode 遍历出的个数，应该正确。

            Assert.IsTrue(component.CountNodes() == countBefore, "只查询一级节点，统计该节点的个数应该正确。");
            Assert.IsTrue(!component.IsFullLoaded, "同时，它的 IsFullLoaded 返回假。");
            int count = 0;
            component.EachNode(e =>
            {
                count++;
                return false;
            });
            Assert.IsTrue(count == countBefore, "此时，调用 EachNode 遍历出的个数，应该正确。");

            component.LoadAllNodes();
            Assert.IsTrue(component.CountNodes() == countAfter, "调用 LoadAllNodes 查询整个树后，统计该节点的个数应该正确。");
            count = 0;
            component.EachNode(e =>
            {
                count++;
                return false;
            });
            Assert.IsTrue(count == countAfter, "此时，调用 EachNode 遍历出的个数，应该正确。");
        }

        #endregion

        #region 结构 - 其它

        /// <summary>
        /// 可以使用 Clone 方法复制一个树节点。
        /// </summary>
        [TestMethod]
        public void TET_Struc_Clone()
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
            foreach (var treeChild in a.TreeChildren)
            {
                Assert.IsTrue(treeChild.IsTreeParentLoaded && treeChild.TreeParent == a);
            }
            var b = list2[1];
            Assert.IsTrue(b.TreeChildren.IsLoaded && b.TreeChildren.Count == 1);
        }

        /// <summary>
        /// 一个新的节点，可以任意在集合中添加、删除。
        /// </summary>
        [TestMethod]
        public void TET_Struc_TreeChildren_AddRemoveNewNode()
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
                }
            };
            var root = list[0];
            var newChild = new Folder();
            root.TreeChildren.Add(newChild);
            root.TreeChildren.Remove(newChild);
            root.TreeChildren.Add(newChild);
            root.TreeChildren.Remove(newChild);
            root.TreeChildren.Add(newChild);
        }

        /// <summary>
        /// 一个新的节点，可以任意在集合中添加、删除。
        /// </summary>
        [TestMethod]
        public void TET_Struc_RootList_AddRemoveNewNode()
        {
            var list = new FolderList
            {
                new Folder()
            };
            var root = list[0];
            var newChild = new Folder();
            list.Add(newChild);
            list.Remove(newChild);
            list.Add(newChild);
            list.Remove(newChild);
            list.Add(newChild);
        }

        /// <summary>
        /// 升级、降级
        /// </summary>
        [TestMethod]
        public void TET_Struc_LevelUp()
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

            var root = list[0];

            //移动
            var node11 = root.TreeChildren[0];
            var node12 = root.TreeChildren[1];
            node11.TreeParent = node12;

            Assert.AreEqual(list.Count, 1);
            Assert.AreEqual(root.TreeIndex, "001.");
            Assert.AreEqual(root.TreeChildren.Count, 1);
            var newNode11 = root.TreeChildren[0];
            Assert.AreEqual(newNode11.TreeIndex, "001.001.");
            Assert.AreEqual(newNode11.TreeChildren.Count, 1);
            Assert.AreEqual(newNode11.TreeChildren[0].TreeIndex, "001.001.001.");
            Assert.AreEqual(newNode11.TreeChildren[0].TreeChildren.Count, 2);
            Assert.AreEqual(newNode11.TreeChildren[0].TreeChildren[0].TreeIndex, "001.001.001.001.");
            Assert.AreEqual(newNode11.TreeChildren[0].TreeChildren[1].TreeIndex, "001.001.001.002.");
        }

        /// <summary>
        /// 把一个子树删除后，子树中的所有节点的状态都应该是删除的。
        /// </summary>
        [TestMethod]
        public void TET_Struc_RemoveTree_Status()
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
                }
            };
            list.MarkSaved();

            var root = list[0];

            list.RemoveAt(0);

            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.DeletedList.Count, 1);
            Assert.IsTrue(root.IsDeleted);
            Assert.IsTrue(root.TreeChildren[0].IsDeleted);
            Assert.IsTrue(root.TreeChildren[1].IsDeleted);
        }

        /// <summary>
        /// 把一个子树删除后，子树中的所有节点的状态都应该是删除的。
        /// </summary>
        [TestMethod]
        public void TET_Struc_RemoveTree_Status_Clear()
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
                }
            };
            list.MarkSaved();

            var root = list[0];

            list.Clear();

            Assert.AreEqual(list.Count, 0);
            Assert.AreEqual(list.DeletedList.Count, 1);
            Assert.IsTrue(root.IsDeleted);
            Assert.IsTrue(root.TreeChildren[0].IsDeleted);
            Assert.IsTrue(root.TreeChildren[1].IsDeleted);
        }

        /// <summary>
        /// 把一个完整删除的子树，添加到根节点列表中后，子树中的所有节点的状态都应该是未删除的。
        /// </summary>
        [TestMethod]
        public void TET_Struc_AddRemovedTreeIntoList()
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
            list.MarkSaved();

            var root = list[0];
            list.Clear();

            (root as ITreeComponent).EachNode(c =>
            {
                Assert.IsTrue(c.IsDeleted);
                return false;
            });

            list.Add(root);

            (root as ITreeComponent).EachNode(c =>
            {
                Assert.IsFalse(c.IsDeleted);
                return false;
            });
        }

        #endregion

        /// <summary>
        /// 贪婪加载子列表时，需要处理子列表是一棵树的情况。&测试。
        /// </summary>
        [TestMethod]
        public void TET_Query_EagerLoadAChildTree()
        {
            var repo  = RF.Concrete<TestUserRepository>();
            using (RF.TransactionScope(repo))
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
                repo.Save(user);

                var newUser = repo.GetWithTasks(user.Id);
                Assert.IsTrue(newUser.GetProperty(TestUser.TestTreeTaskListProperty) != null);
                var tasks = newUser.TestTreeTaskList;

                Assert.AreEqual(tasks.Count, 1);
                Assert.AreEqual(tasks[0].TreeIndex, "001.");
                Assert.IsTrue(tasks[0].TreeChildren.IsLoaded);
                Assert.AreEqual(tasks[0].TreeChildren.Count, 2);
                Assert.AreEqual(tasks[0].TreeChildren[0].TreeIndex, "001.001.");
                Assert.IsTrue(tasks[0].TreeChildren[0].TreeChildren.IsLoaded);
                Assert.AreEqual(tasks[0].TreeChildren[0].TreeChildren.Count, 2);
                Assert.AreEqual(tasks[0].TreeChildren[0].TreeChildren[0].TreeIndex, "001.001.001.");
                Assert.AreEqual(tasks[0].TreeChildren[0].TreeChildren[1].TreeIndex, "001.001.002.");
                Assert.AreEqual(tasks[0].TreeChildren[1].TreeIndex, "001.002.");
            }
        }
    }
}