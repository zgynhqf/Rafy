using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.DbMigration;
using Rafy.Domain;
using System.Transactions;
using Rafy.RBAC;

namespace JXC.DbMigrations
{
    public class _20120423_183700_InitStorage : DataMigration
    {
        protected override string GetDescription()
        {
            return "添加 仓库 的初始数据。";
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var repo = RF.Find<Storage>();
                var list = repo.GetAll();
                if (list.Count == 0)
                {
                    repo.Save(new Storage
                    {
                        Code = "20120423-0001",
                        Name = "北京仓库",
                        Address = "北京",
                        ResponsiblePerson = "胡庆访",
                        Area = "海淀区",
                        IsDefault = true
                    });

                    repo.Save(new Storage
                    {
                        Code = "20120425-0002",
                        Name = "云南仓库",
                        Address = "云南省",
                        ResponsiblePerson = "胡庆访",
                        Area = "昭通市"
                    });
                }
            });
        }
    }
}
