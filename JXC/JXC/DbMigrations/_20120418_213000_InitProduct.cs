using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration;
using OEA.Library;
using System.Transactions;
using OEA.RBAC;

namespace JXC.DbMigrations
{
    public class _20120418_213000_InitProduct : DataMigration
    {
        protected override string GetDescription()
        {
            return "添加 商品 的初始数据。";
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var productRepo = RF.Create<Product>();
                var list = productRepo.GetAll();
                if (list.Count == 0)
                {
                    var cateRepo = RF.Create<ProductCategory>();
                    var clientRepo = RF.Create<ClientInfo>();
                    var userRepo = RF.Create<User>();

                    var categories = cateRepo.GetAll();
                    var clients = clientRepo.GetAll();
                    var operators = userRepo.GetAll();

                    var cate1 = categories.Cast<ProductCategory>().First(c => c.Name == "服饰类");
                    var cate2 = categories.Cast<ProductCategory>().First(c => c.Name == "食品类");
                    var client1 = clients.Cast<ClientInfo>().First(c => c.Name == "新新服装加工厂");
                    var client2 = clients.Cast<ClientInfo>().First(c => c.Name == "乐多食品加工厂");
                    var admin = operators[0] as User;

                    list.Add(new Product
                    {
                        ProductCategory = cate1,
                        Supplier = client1,
                        Operator = admin,
                        BianMa = "200909150009",
                        MingCheng = "运动鞋",
                        GuiGe = "双",
                        PingPai = "安踏",
                        CaiGouDanjia = 85.00,
                        XiaoShouDanJia = 165.00,
                        OperateTime = DateTime.Now,
                        StorageAmount = 1000
                    });
                    list.Add(new Product
                    {
                        ProductCategory = cate1,
                        Supplier = client1,
                        Operator = admin,
                        BianMa = "200909150005",
                        MingCheng = "休闲裤",
                        GuiGe = "件",
                        PingPai = "AF",
                        CaiGouDanjia = 35.00,
                        XiaoShouDanJia = 55.00,
                        OperateTime = DateTime.Now,
                        StorageAmount = 1000
                    });

                    list.Add(new Product
                    {
                        ProductCategory = cate2,
                        Supplier = client2,
                        Operator = admin,
                        BianMa = "200909150013",
                        MingCheng = "零度可口可乐",
                        GuiGe = "瓶",
                        PingPai = "可口可乐",
                        CaiGouDanjia = 1.50,
                        XiaoShouDanJia = 2.50,
                        OperateTime = DateTime.Now,
                        StorageAmount = 1000
                    });
                    list.Add(new Product
                    {
                        ProductCategory = cate2,
                        Supplier = client2,
                        Operator = admin,
                        BianMa = "200909150014",
                        MingCheng = "雪碧",
                        GuiGe = "瓶",
                        PingPai = "可口可乐",
                        CaiGouDanjia = 1.00,
                        XiaoShouDanJia = 2.00,
                        OperateTime = DateTime.Now,
                        StorageAmount = 1000
                    });
                    list.Add(new Product
                    {
                        ProductCategory = cate2,
                        Supplier = client2,
                        Operator = admin,
                        BianMa = "200909150016",
                        MingCheng = "益力多",
                        GuiGe = "瓶",
                        PingPai = "益力多",
                        CaiGouDanjia = 1.20,
                        XiaoShouDanJia = 2.00,
                        OperateTime = DateTime.Now,
                        StorageAmount = 1000
                    });

                    productRepo.Save(list);
                }
            });
        }
    }
}
