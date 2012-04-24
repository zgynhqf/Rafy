using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration;
using OEA.Library;
using System.Transactions;

namespace JXC.DbMigrations
{
    public class _20120417_142100_InitClient : DataMigration
    {
        protected override string GetDescription()
        {
            return "添加 用户 的初始数据。";
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                var ccRepo = RF.Concreate<ClientCategoryRepository>();
                var cclist = ccRepo.GetAll(false);
                if (cclist.Count == 0)
                {
                    using (var tran = RF.TransactionScope(ccRepo))
                    {
                        var supplier = new ClientCategory { Name = ClientCategory.SupplierName };
                        var customer = new ClientCategory { Name = ClientCategory.CustomerName };
                        RF.Save(supplier);
                        RF.Save(customer);

                        var clientRepo = RF.Concreate<ClientInfoRepository>();
                        clientRepo.Save(new ClientInfo
                        {
                            ClientCategory = supplier,
                            Name = "新新服装加工厂",
                            FaRenDaiBiao = "新新",
                            YouXiang = "xinxin@yahoo.com.cn",
                            KaiHuYinHang = "中国银行",
                        });
                        clientRepo.Save(new ClientInfo
                        {
                            ClientCategory = supplier,
                            Name = "乐多食品加工厂",
                            FaRenDaiBiao = "乐多",
                            YouXiang = "leiduo@163.com.cn",
                            KaiHuYinHang = "中国银行",
                        });
                        clientRepo.Save(new ClientInfo
                        {
                            ClientCategory = supplier,
                            Name = "腾飞服装有限公司",
                            FaRenDaiBiao = "腾飞",
                            YouXiang = "tengfei@gmail.com.cn",
                            KaiHuYinHang = "中国银行",
                        });
                        clientRepo.Save(new ClientInfo
                        {
                            ClientCategory = customer,
                            Name = "好又多商场",
                            FaRenDaiBiao = "李海",
                            ShouJiaJiBie = ShouJiaJiBie.JiBie_2,
                            YouXiang = "lihai@gmail.com.cn",
                            KaiHuYinHang = "中国银行",
                        });

                        tran.Complete();
                    }
                }
            });
        }
    }
}
