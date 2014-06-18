using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.DbMigration;
using Rafy.Domain;
using Rafy;

namespace FM.DbMigrations
{
    public class _20120429_151300_InitUserAndTags : DataMigration
    {
        protected override string GetDescription()
        {
            return "添加 用户、标签 的初始数据。";
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var repo = RF.Concrete<PersonRepository>();
                var list = repo.GetAll();
                if (list.Count == 0)
                {
                    list.Add(new Person
                    {
                        Name = "胡庆访",
                        IsDefault = true
                    });

                    var tagRepo = RF.Find<Tag>();
                    var tagList = tagRepo.NewList();
                    tagList.Add(new Tag { Name = "衣" });
                    tagList.Add(new Tag { Name = "食" });
                    tagList.Add(new Tag { Name = "住" });
                    tagList.Add(new Tag { Name = "行" });
                    tagList.Add(new Tag { Name = "人情" });
                    tagList.Add(new Tag { Name = "公司" });
                    tagList.Add(new Tag { Name = "工资" });

                    using (var tran = RF.TransactionScope(repo))
                    {
                        repo.Save(list);
                        tagRepo.Save(tagList);

                        tran.Complete();
                    }
                }
            });
        }
    }
}
