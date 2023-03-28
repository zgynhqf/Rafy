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
#if NET6_0 //因为 MongoDb.Driver 支持的 NETFramework 是 4.7.2 以上版本。所以 NET461 不需要运行以下测试。
    [TestClass]
    public class MongoDbTest
    {
        public static bool MongoDbEnabled = false;

        [ClassInitialize]
        public static void MongoT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
            MongoDbEnabled = ConfigurationHelper.GetConnectionString("Test_MongoDb") != null;
        }

        [TestMethod]
        public void MongoT_Create()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Id));

                var saved = repo.GetFirst();
                Assert.IsNotNull(saved);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(saved.Id));

                Assert.AreEqual(item.Code, saved.Code);
                Assert.AreEqual(item.AdjustTime, saved.AdjustTime);
                Assert.AreEqual(item.CurrentCash, saved.CurrentCash);
                Assert.AreEqual(item.LastTradeDate, saved.LastTradeDate);
                Assert.AreEqual(true, saved.Enabled);
                Assert.AreEqual(false, saved.RearrangePosition);
                Assert.AreEqual(false, saved.SellIfHitHighLimit);
                Assert.AreEqual(false, saved.BuyIfHitLowLimit);
                Assert.AreEqual(item.GrId, saved.GrId);
                Assert.AreEqual(1, saved.GetDynamicProperty("dynamicPropety"));
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Delete()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);

                var saved = repo.GetFirst();
                Assert.IsNotNull(saved);

                saved.PersistenceStatus = PersistenceStatus.Deleted;
                repo.Save(saved);

                Assert.AreEqual(0, repo.CountAll());
            }
            catch
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Delete_List()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);

                var saved = repo.GetAll();
                Assert.AreEqual(1, saved.Count);

                saved.Clear();
                repo.Save(saved);

                Assert.AreEqual(0, repo.CountAll());
            }
            catch
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Update()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);

                item.Code = "502";
                item.CurrentCash = 1;
                item.LastTradeDate = new DateTime(2000, 1, 1);
                item.Enabled = false;
                item.SetDynamicProperty("dynamicPropety", 2);

                item.Stocks[0].HoldNumber = 1111;

                item.Stocks.RemoveAt(1);
                item.Stocks.Add(new StockCombinationItem
                {
                    StockCode = "000003",
                    HoldNumber = 3000
                });
                item.Stocks.Add(new StockCombinationItem
                {
                    StockCode = "000004",
                    HoldNumber = 4000
                });
                repo.Save(item);

                var saved = repo.GetFirst();
                Assert.IsNotNull(saved);
                Assert.AreEqual(item.Code, saved.Code);
                Assert.AreEqual(item.CurrentCash, saved.CurrentCash);
                Assert.AreEqual(item.LastTradeDate, saved.LastTradeDate);
                Assert.AreEqual(false, saved.Enabled);
                Assert.AreEqual(2, saved.GetDynamicProperty("dynamicPropety"));
                Assert.AreEqual(1111, saved.Stocks[0].HoldNumber);
                Assert.AreEqual(3, saved.Stocks.Count);
                Assert.AreEqual("000003", saved.Stocks[1].StockCode);
                Assert.AreEqual(3000, saved.Stocks[1].HoldNumber);
                Assert.AreEqual("000004", saved.Stocks[2].StockCode);
                Assert.AreEqual(4000, saved.Stocks[2].HoldNumber);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Update_OnlyOnRoot()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);

                var stockItem = item.Stocks[0];
                stockItem.HoldNumber = 1111;

                bool success = false;
                try
                {
                    var itemRepo = RF.ResolveInstance<StockCombinationItemRepository>();
                    itemRepo.Save(stockItem);
                    success = true;
                }
                catch (Exception)
                {
                }

                if (success)
                {
                    throw new Exception("不能使用子实体的仓库来进行保存操作。");
                }
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_GetAll()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            var list = repo.GetAll();
            Assert.AreEqual(0, list.Count);

            try
            {
                CreateDemoData(repo);

                list = repo.GetAll();
                Assert.AreEqual(1, list.Count);
                Assert.AreEqual(PersistenceStatus.Saved, list[0].PersistenceStatus);
                Assert.AreEqual(2, list[0].Stocks.Count);
                Assert.AreEqual(PersistenceStatus.Saved, list[0].Stocks[0].PersistenceStatus);
                Assert.AreEqual(PersistenceStatus.Saved, list[0].Stocks[1].PersistenceStatus);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_GetAll_Paging()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();

            try
            {
                CreateDemoData(repo, "501");
                CreateDemoData(repo, "502");
                CreateDemoData(repo, "503");

                var pi = new PagingInfo(1, 2, true);
                var list = repo.GetAll(pi);

                Assert.AreEqual(3, pi.TotalCount);
                Assert.AreEqual(2, list.Count);

                pi.PageNumber = 2;
                list = repo.GetAll(pi);
                Assert.AreEqual(1, list.Count);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_Sort()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();

            try
            {
                CreateDemoData(repo, "501");
                CreateDemoData(repo, "503");

                var list = repo.GetBy(new CommonQueryCriteria
                {
                    OrderBy = StockCombination.CodeProperty.Name,
                    OrderByAscending = false
                });

                Assert.AreEqual(2, list.Count);
                Assert.AreEqual("503", list[0].Code);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_CountAll()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            Assert.AreEqual(0, repo.CountAll());

            try
            {
                StockCombination item = CreateDemoData(repo);

                Assert.AreEqual(1, repo.CountAll());
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_GetByMatchSingleProperty()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                StockCombination item = CreateDemoData(repo);

                var saved = repo.GetByCode("501");
                Assert.IsNotNull(saved);
                Assert.AreEqual("501", item.Code);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_Query_SearchBySingleProperty()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                repo.Save(new StockCombination { Code = "101", CurrentCash = 101 });
                repo.Save(new StockCombination { Code = "102", CurrentCash = 102 });
                repo.Save(new StockCombination { Code = "103", CurrentCash = 103 });
                repo.Save(new StockCombination { Code = "201", CurrentCash = 201 });

                Assert.AreEqual(1, SearchBy(repo, new PropertyMatch(StockCombination.CodeProperty, PropertyOperator.Equal, "101")).Count);
                Assert.AreEqual(3, SearchBy(repo, new PropertyMatch(StockCombination.CodeProperty, PropertyOperator.NotEqual, "101")).Count);
                Assert.AreEqual(1, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.Equal, 101)).Count);
                Assert.AreEqual(3, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.NotEqual, 101)).Count);

                Assert.AreEqual(2, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.Greater, 102)).Count);
                Assert.AreEqual(3, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.GreaterEqual, 102)).Count);
                Assert.AreEqual(2, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.Less, 103)).Count);
                Assert.AreEqual(3, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.LessEqual, 103)).Count);

                Assert.AreEqual(2, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.In, new int[] { 101, 103 })).Count);
                Assert.AreEqual(2, SearchBy(repo, new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.NotIn, new int[] { 101, 103 })).Count);

                //字符串模糊匹配，暂时不支持
                //Assert.AreEqual(1, SearchBy(repo, new PropertyMatch(StockCombination.CodeProperty, PropertyOperator.Like, "101")).Count);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        [TestMethod]
        public void MongoT_SearchByMultipleProperties()
        {
            if (!MongoDbEnabled) return;

            var repo = RF.ResolveInstance<StockCombinationRepository>();
            try
            {
                repo.Save(new StockCombination { Code = "101", CurrentCash = 101 });
                repo.Save(new StockCombination { Code = "101", CurrentCash = 102 });
                repo.Save(new StockCombination { Code = "102", CurrentCash = 102 });
                repo.Save(new StockCombination { Code = "102", CurrentCash = 101 });

                var criteria = new CommonQueryCriteria
                {
                    new PropertyMatch(StockCombination.CodeProperty, PropertyOperator.Equal, "101"),
                    new PropertyMatch(StockCombination.CurrentCashProperty, PropertyOperator.Equal, 101),
                };
                var list = repo.GetBy(criteria);
                Assert.AreEqual(1, list.Count);

                criteria.Concat = BinaryOperator.Or;
                list = repo.GetBy(criteria);
                Assert.AreEqual(3, list.Count);
            }
            finally
            {
                this.DeleteAllDatas();
            }
        }

        //public StockCombinationList SearchByCodeCompare(StockCombinationRepository repo, PropertyOperator op, string code)
        public StockCombinationList SearchBy(StockCombinationRepository repo, PropertyMatch propertyMatch)
        {
            return repo.GetBy(new CommonQueryCriteria
            {
                propertyMatch,
            });
        }

        private static StockCombination CreateDemoData(StockCombinationRepository repo, string code = "501")
        {
            var item = new StockCombination
            {
                Code = code,
                AdjustTime = "09:45:00",
                CurrentCash = 0,
                LastTradeDate = new DateTime(2020, 1, 1),
                GrId = "000000001",
                Stocks =
                {
                    new StockCombinationItem
                    {
                        StockCode = "000001",
                        HoldNumber = 1000
                    },
                    new StockCombinationItem
                    {
                        StockCode = "000002",
                        HoldNumber = 2000
                    }
                }
            };
            item.SetDynamicProperty("dynamicPropety", 1);
            repo.Save(item);
            return item;
        }

        private void DeleteAllDatas()
        {
            var repo = RF.ResolveInstance<StockCombinationRepository>();
            var list = repo.GetAll();
            list.Clear();
            repo.Save(list);
            Assert.AreEqual(0, repo.CountAll());
        }
    }
#endif
}