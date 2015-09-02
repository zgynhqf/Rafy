/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140515
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140515 20:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Domain;
using UT;

namespace RafyUnitTest.StringEntity
{
    [TestClass]
    public class EntityIdTest
    {
        [ClassInitialize]
        public static void EIT_ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void EIT_StringId_Property_Id_Default()
        {
            var entity = new House();
            Assert.IsTrue(entity.Id == string.Empty);
        }

        [TestMethod]
        public void EIT_StringId_Insert()
        {
            var repo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new House
                {
                    Id = "House1"
                });
                var count = repo.CountAll();
                Assert.IsTrue(count == 1);
            }
        }

        [TestMethod]
        public void EIT_StringId_Insert_NullKey()
        {
            var repo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                bool error = false;
                try
                {
                    repo.Save(new House { Id = null });
                }
                catch
                {
                    error = true;
                }
                Assert.IsTrue(error, "没有设置 StringEntity 的主键，必须报错。");
            }
        }

        [TestMethod]
        public void EIT_StringId_Insert_DeplicateKey()
        {
            var repo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                repo.Save(new House { Id = "Id1" });

                bool error = false;
                try
                {
                    repo.Save(new House { Id = "Id1" });
                }
                catch
                {
                    error = true;
                }
                Assert.IsTrue(error, "重复的主键，必须报错。");
            }
        }

        [TestMethod]
        public void EIT_StringId_StringRefLong()
        {
            var lRepo = RF.Concrete<LesseeRepository>();
            var repo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                var house = new House { Id = "House1" };
                repo.Save(house);

                house = repo.GetFirst();
                Assert.IsTrue(house.LesseeId == null);

                var lessee = new Lessee();
                lRepo.Save(lessee);
                Assert.IsTrue(lessee.Id > 0);

                house.Lessee = lessee;
                Assert.IsTrue(house.LesseeId.GetValueOrDefault() == lessee.Id);

                repo.Save(house);
                house = repo.GetFirst();
                Assert.IsTrue(house.LesseeId.GetValueOrDefault() == lessee.Id);
            }
        }

        [TestMethod]
        public void EIT_StringId_IntWithString_SetParentId()
        {
            var merchant = new HouseMerchant
            {
                MerchantItemList =
                {
                    new MerchantItem()
                }
            };
            merchant.Id = "Merchant1";
            Assert.IsTrue(merchant.MerchantItemList[0].HouseMerchantId == merchant.Id);
        }

        [TestMethod]
        public void EIT_StringId_IntWithString_Insert()
        {
            var repo = RF.Concrete<HouseMerchantRepository>();
            var houseRepo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                var house = new House { Id = "House1" };
                houseRepo.Save(house);
                var house2 = new House { Id = "House2" };
                houseRepo.Save(house2);

                var merchant = new HouseMerchant
                {
                    MerchantItemList =
                    {
                        new MerchantItem
                        {
                            House = house
                        },
                        new MerchantItem
                        {
                            House = house2
                        }
                    }
                };
                merchant.Id = "Merchant1";
                repo.Save(merchant);
            }
        }

        [TestMethod]
        public void EIT_StringId_IntWithString_GetByParentString()
        {
            var repo = RF.Concrete<HouseMerchantRepository>();
            var houseRepo = RF.Concrete<HouseRepository>();
            using (RF.TransactionScope(repo))
            {
                var house = new House { Id = "House1" };
                houseRepo.Save(house);
                var house2 = new House { Id = "House2" };
                houseRepo.Save(house2);

                var merchant = new HouseMerchant
                {
                    Id = "Merchant1",
                    MerchantItemList =
                    {
                        new MerchantItem
                        {
                            House = house
                        },
                        new MerchantItem
                        {
                            House = house2
                        }
                    }
                };
                repo.Save(merchant);

                merchant = repo.GetFirst();
                Assert.IsTrue(merchant.MerchantItemList.Count == 2);
                Assert.IsTrue(merchant.MerchantItemList[0].HouseId == house.Id);
                Assert.IsTrue(merchant.MerchantItemList[1].HouseId == house2.Id);
            }
        }

        [TestMethod]
        public void EIT_StringId_IntWithString_GetParentRef()
        {
            var repo = RF.Concrete<HouseMerchantRepository>();
            var itemRepo = RF.Concrete<MerchantItemRepository>();
            using (RF.TransactionScope(repo))
            {
                var house = new House { Id = "House1" };
                RF.Save(house);

                var merchant = new HouseMerchant
                {
                    MerchantItemList =
                    {
                        new MerchantItem
                        {
                            House = house
                        }
                    }
                };
                merchant.Id = "Merchant1";
                repo.Save(merchant);

                var item = itemRepo.GetFirst();
                Assert.IsTrue(item.HouseMerchantId == merchant.Id);
            }
        }

        [TestMethod]
        public void EIT_LongId_Property_Id_Default()
        {
            var entity = new Lessee();
            Assert.IsTrue(entity.Id == 0);
        }

        [TestMethod]
        public void EIT_LongId_Insert()
        {
            var repo = RF.Concrete<LesseeRepository>();
            using (RF.TransactionScope(repo))
            {
                var entity = new Lessee();
                repo.Save(entity);
                var count = repo.CountAll();
                Assert.IsTrue(count == 1);

                var entity2 = repo.GetById(entity.Id);
                Assert.IsNotNull(entity2);
            }
        }
    }
}