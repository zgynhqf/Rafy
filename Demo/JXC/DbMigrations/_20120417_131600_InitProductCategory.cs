using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.DbMigration;
using Rafy.Domain;
using System.Transactions;
using Rafy;

namespace JXC.DbMigrations
{
    public class _20120417_131600_InitProduct : DataMigration
    {
        public override string Description
        {
            get { return "添加 商品类别 的初始数据。"; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var repo = RF.Concrete<ProductCategoryRepository>();
                var list = repo.GetAll();
                if (list.Count == 0)
                {
                    list.Add(new ProductCategory
                    {
                        Id = RafyEnvironment.NewLocalId(),
                        Name = "服饰类",
                        TreeChildren ={
                            new ProductCategory{ Name = "裤子" },
                            new ProductCategory{ Name = "裙子" },
                            new ProductCategory{ Name = "上衣" },
                            new ProductCategory{ Name = "鞋子" },
                        }
                    });
                    list.Add(new ProductCategory
                    {
                        Id = RafyEnvironment.NewLocalId(),
                        Name = "食品类",
                        TreeChildren ={
                            new ProductCategory{ Name = "生鲜食品" },
                            new ProductCategory{ Name = "饮料" },
                        }
                    });

                    using (var tran = RF.TransactionScope(repo))
                    {
                        repo.Save(list);

                        tran.Complete();
                    }
                }
            });
        }
    }
}
